namespace OpenLdapCs.Models;

public sealed class RefreshTokenRecord
{
    public required string Username { get; set; }
    public required string DistinguishedName { get; set; }
    public required DateTime ExpiresAtUtc { get; set; }
}
