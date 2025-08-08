namespace WebApi.Entities;

public class Click(long id, string shortUrl, string ip, string userAgent, DateTime timeStamp)
{
    public long Id { get; set; } = id;
    public string ShortUrl { get; set; } = shortUrl;
    public string Ip { get; set; } = ip;
    public string UserAgent { get; set; } = userAgent; 
    public DateTime Timestamp { get; set; } = timeStamp;
}