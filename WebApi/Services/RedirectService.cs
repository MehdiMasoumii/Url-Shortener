using MongoDB.Driver;
using WebApi.Persistence;
using WebApi.Services.Redis;

namespace WebApi.Services;

public class RedirectService(
    AppDbContext dbContext,
    UrlCacheService urlCacheService
    )
{
    public async Task<string?> GetOriginalUrl(string shortUrl)
    {
        var cachedUrl = await urlCacheService.GetOriginalUrlAsync(shortUrl);
        if (cachedUrl != null) return cachedUrl;
        
        var originalUrl = await dbContext.Urls.Find(u => u.ShortUrl == shortUrl).Project(u => u.OriginalUrl).FirstOrDefaultAsync();
        await urlCacheService.CacheUrlAsync(shortUrl,originalUrl, TimeSpan.FromHours(1));
        
        return originalUrl;
    }

    public async Task CacheTopClickUrlsAsync()
    {
        await urlCacheService.CacheTopClickUrlsAsync(50);
    }
}