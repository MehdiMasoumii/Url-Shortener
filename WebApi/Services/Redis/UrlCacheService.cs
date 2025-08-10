using MongoDB.Driver;
using MongoDB.Driver.Linq;
using StackExchange.Redis;
using WebApi.Dtos.Analytics;
using WebApi.Persistence;

namespace WebApi.Services.Redis;

public class UrlCacheService(IDatabase redisContext, ILogger<UrlCacheService> logger, AppDbContext dbContext)
{
    public async Task<string?> GetOriginalUrlAsync(string shortUrl)
    {
        try
        {
            var cacheKey = $"url:{shortUrl}";
            var cachedUrl = await redisContext.StringGetSetExpiryAsync(cacheKey, TimeSpan.FromHours(1));

            if (cachedUrl.HasValue)
            {
                logger.LogInformation("Cache hit for {shortUrl}", shortUrl);
                return cachedUrl.ToString();
            }

            logger.LogInformation("Cache miss for {shortUrl}", shortUrl);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error getting url from cache");
            return null;
        }

    }

    public async Task CacheUrlAsync(string shortUrl, string url, TimeSpan? expiry = null)
    {
        try
        {
            var cacheKey = $"url:{shortUrl}";
            await redisContext.StringSetAsync(cacheKey, url, expiry);
            logger.LogInformation("Cached url for {shortUrl}", shortUrl);
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error caching url");
            throw;
        }
        
    }

    public async Task CacheTopClickUrlsAsync(int count)
    {
        var clicksQ = dbContext.Clicks.AsQueryable();
        var urlsQ = dbContext.Urls.AsQueryable();

        var groupedClicks = from click in clicksQ
            group click by click.ShortUrl
            into g
            select new
            {
                ShortUrl = g.Key,
                Count = g.Count()
            };
        
        var query = from gc in groupedClicks
            join url in urlsQ on gc.ShortUrl equals url.ShortUrl
            orderby gc.Count descending
            select new TopClick
            {
                ShortUrl = gc.ShortUrl,
                OriginalUrl = url.OriginalUrl,
                Count = gc.Count
            } into result
            orderby result.Count descending
            select result;

        try
        {
            var topClicks = await query.Take(count).ToListAsync();
            var keyValuePair = topClicks
                .Select(c => new KeyValuePair<RedisKey, RedisValue>($"url:{c.ShortUrl}", c.OriginalUrl))
                .ToArray();

            await redisContext.StringSetAsync(keyValuePair);
            logger.LogInformation("Top {count} high click urls have cached!", count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to cache top click urls!");
            throw;
        }
    }
}