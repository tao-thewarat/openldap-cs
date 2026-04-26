namespace OpenLdapCs.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SecretKey { get; set; }
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
