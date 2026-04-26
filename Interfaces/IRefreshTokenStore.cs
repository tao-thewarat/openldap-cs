using OpenLdapCs.Models;

namespace OpenLdapCs.Interfaces;

public interface IRefreshTokenStore
{
    Task StoreAsync(
        string refreshToken,
        RefreshTokenRecord record,
        TimeSpan timeToLive,
        CancellationToken cancellationToken = default
    );

    Task<RefreshTokenRecord?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(string refreshToken, CancellationToken cancellationToken = default);
}
