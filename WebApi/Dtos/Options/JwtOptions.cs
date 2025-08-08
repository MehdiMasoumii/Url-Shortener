namespace WebApi.Dtos.Options;

public record JwtOptions
{
    public required string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int RefreshTokenLifeTimeDay { get; set; }
    public int AccessTokenLifeTimeMin { get; set; }
};