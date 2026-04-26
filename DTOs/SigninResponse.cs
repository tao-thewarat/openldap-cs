using System.Text.Json.Serialization;

namespace OpenLdapCs.DTOs;

public class SigninResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }

    [JsonIgnore]
    public string? AccessToken { get; set; }

    [JsonIgnore]
    public string? RefreshToken { get; set; }
}
