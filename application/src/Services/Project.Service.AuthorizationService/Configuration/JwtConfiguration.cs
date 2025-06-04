namespace Project.Service.AuthorizationService.Configuration;

public class JwtConfiguration
{
    public string Issuer { get; set; }
    public string Audience { get; set; }

    public int AccessTokenLifetimeMinutes { get; set; }

    public int RefreshTokenLifetimeDays { get; set; }

    public string SecurityKey { get; set; }
}