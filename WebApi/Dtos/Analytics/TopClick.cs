namespace WebApi.Dtos.Analytics;

public record TopClick
{
    public string ShortUrl { get; set; }
    public string OriginalUrl { get; set; }
    public int Count { get; set; }
};
