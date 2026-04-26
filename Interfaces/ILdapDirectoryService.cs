namespace OpenLdapCs.Interfaces;

public interface ILdapDirectoryService
{
    Task<string?> FindUserDnAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ValidateCredentialsAsync(
        string distinguishedName,
        string password,
        CancellationToken cancellationToken = default
    );

    Task<bool> UserExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<string> CreateUserAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default
    );
}
