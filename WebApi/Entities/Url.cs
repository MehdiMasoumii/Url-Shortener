namespace WebApi.Entities;

public class Url
{
    public long Id { get; set; }
    public string ShortUrl { get; set; }
    public string OriginalUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public long OwnerId { get; set; }

    public Url(long id, long ownerId ,string shortUrl, string originalUrl)
    {
        Id = id;
        ShortUrl = shortUrl;
        OriginalUrl = originalUrl;
        OwnerId = ownerId;
        CreatedDate = DateTime.UtcNow;
        IsActive = true;
    }
}