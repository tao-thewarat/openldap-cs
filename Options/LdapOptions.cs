namespace OpenLdapCs.Options;

public sealed class LdapOptions
{
    public const string SectionName = "Ldap";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 389;
    public bool UseSsl { get; set; }
    public string BaseDn { get; set; } = "dc=example,dc=org";
    public string UsersDn { get; set; } = "ou=people,dc=example,dc=org";
    public string BindDn { get; set; } = "cn=admin,dc=example,dc=org";
    public string BindPassword { get; set; } = "admin";
}
