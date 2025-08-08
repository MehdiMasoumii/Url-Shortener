using WebApi.Entities;
using WebApi.Events;
using WebApi.Services;

namespace WebApi.Persistence.Repository;

public class ClickRepository(AppDbContext dbContext, IdGeneratorService idGeneratorService)
{
    public async Task SaveClickAsync(ClickEvent clickEvent)
    {
        var click = new Click(
            idGeneratorService.GenerateId(),
            clickEvent.ShortUrl,
            clickEvent.Ip,
            clickEvent.UserAgent,
            clickEvent.Timestamp
            );
        await dbContext.Clicks.InsertOneAsync(click);
    }
}