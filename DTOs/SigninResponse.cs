namespace OpenLdapCs.DTOs;

public class SigninResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public string? DistinguishedName { get; set; }
}
