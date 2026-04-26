using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Options;
using OpenLdapCs.Interfaces;
using OpenLdapCs.Options;

namespace OpenLdapCs.Services;

public sealed class LdapDirectoryService : ILdapDirectoryService
{
    private readonly LdapOptions options;

    public LdapDirectoryService(IOptions<LdapOptions> options)
    {
        this.options = options.Value;
    }

    public async Task<string?> FindUserDnAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(
            () =>
            {
                using var connection = CreateConnection();
                BindAsAdmin(connection);

                var request = new SearchRequest(
                    options.BaseDn,
                    $"(&(objectClass=inetOrgPerson)(uid={EscapeLdapFilterValue(username)}))",
                    SearchScope.Subtree,
                    null
                );

                var response = (SearchResponse)connection.SendRequest(request);
                return response.Entries.Count == 0 ? null : response.Entries[0].DistinguishedName;
            },
            cancellationToken
        );
    }

    public async Task<bool> ValidateCredentialsAsync(
        string distinguishedName,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(
            () =>
            {
                try
                {
                    using var connection = CreateConnection();
                    Bind(connection, distinguishedName, password);
                    return true;
                }
                catch (LdapException)
                {
                    return false;
                }
            },
            cancellationToken
        );
    }

    public async Task<bool> UserExistsAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        var distinguishedName = await FindUserDnAsync(username, cancellationToken);
        return !string.IsNullOrWhiteSpace(distinguishedName);
    }

    public async Task<string> CreateUserAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        return await Task.Run(
            () =>
            {
                using var connection = CreateConnection();
                BindAsAdmin(connection);

                var distinguishedName = $"uid={EscapeDnValue(username)},{options.UsersDn}";
                var attributes = new DirectoryAttribute[]
                {
                    new("objectClass", "top", "person", "organizationalPerson", "inetOrgPerson"),
                    new("cn", username),
                    new("sn", username),
                    new("uid", username),
                    new("mail", $"{username}@example.org"),
                    new("userPassword", password),
                };

                var request = new AddRequest(distinguishedName, attributes);
                connection.SendRequest(request);
                return distinguishedName;
            },
            cancellationToken
        );
    }

    private LdapConnection CreateConnection()
    {
        var identifier = new LdapDirectoryIdentifier(options.Host, options.Port);
        var connection = new LdapConnection(identifier) { AuthType = AuthType.Basic };

        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.SecureSocketLayer = options.UseSsl;
        connection.Timeout = TimeSpan.FromSeconds(10);

        return connection;
    }

    private void BindAsAdmin(LdapConnection connection)
    {
        Bind(connection, options.BindDn, options.BindPassword);
    }

    private static void Bind(LdapConnection connection, string distinguishedName, string password)
    {
        connection.Bind(new NetworkCredential(distinguishedName, password));
    }

    private static string EscapeLdapFilterValue(string value)
    {
        return value
            .Replace("\\", "\\5c", StringComparison.Ordinal)
            .Replace("*", "\\2a", StringComparison.Ordinal)
            .Replace("(", "\\28", StringComparison.Ordinal)
            .Replace(")", "\\29", StringComparison.Ordinal)
            .Replace("\0", "\\00", StringComparison.Ordinal);
    }

    private static string EscapeDnValue(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace(",", "\\,", StringComparison.Ordinal)
            .Replace("+", "\\+", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("<", "\\<", StringComparison.Ordinal)
            .Replace(">", "\\>", StringComparison.Ordinal)
            .Replace(";", "\\;", StringComparison.Ordinal)
            .Replace("=", "\\=", StringComparison.Ordinal);
    }
}
