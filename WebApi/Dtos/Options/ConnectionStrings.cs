namespace WebApi.Dtos.Options;

public record ConnectionStrings
{
    public required string MongoDb { get; init; }
};