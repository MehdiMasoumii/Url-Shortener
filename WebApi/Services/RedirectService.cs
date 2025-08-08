using MongoDB.Driver;
using WebApi.Entities;
using WebApi.Persistence;

namespace WebApi.Services;

public class RedirectService(
    AppDbContext dbContext
    )
{
    public async Task<Url?> GetOriginalUrl(string shortUrl)
    {
        var result = await dbContext.Urls.Find(u => u.ShortUrl == shortUrl).FirstOrDefaultAsync();
        return result;
    } 
}