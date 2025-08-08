using SimpleBase;
using WebApi.Entities;
using WebApi.Persistence.Repository;

namespace WebApi.Services;

public class UrlShortenerService(IdGeneratorService generator, UrlRepository urlRepository)
{
    public async Task<Url> ShortenUrl( long ownerId, string originalUrl)
    {
        var newUrlId = generator.GenerateId();
        var shortUrl = Base62.LowerFirst.Encode(BitConverter.GetBytes(newUrlId));
        
        var newUrl = new Url(newUrlId, ownerId,shortUrl, originalUrl);
        await urlRepository.CreateUrl(newUrl);
        
        return newUrl;
    }
}