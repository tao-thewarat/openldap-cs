namespace OpenLdapCs.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string username, string distinguishedName);
    string GenerateRefreshToken();
}
