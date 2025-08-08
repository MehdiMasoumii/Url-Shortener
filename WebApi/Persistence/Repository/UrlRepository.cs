using WebApi.Entities;

namespace WebApi.Persistence.Repository;

public class UrlRepository(
    AppDbContext dbContext
    )
{
    public async Task CreateUrl(Url url)
    {
        await dbContext.Urls.InsertOneAsync(url);
    }
}