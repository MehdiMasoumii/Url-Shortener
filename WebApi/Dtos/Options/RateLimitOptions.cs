namespace WebApi.Dtos.Options;

public record RateLimitOptions
{
    public int BucketSize { get; set; }
    public int RefillTimeSeconds { get; set; }
};