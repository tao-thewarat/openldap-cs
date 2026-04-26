using OpenLdapCs.DTOs;

namespace OpenLdapCs.Interfaces;

public interface ILdapAuthService
{
    Task<SigninResponse> SigninAsync(
        SigninRequest request,
        CancellationToken cancellationToken = default
    );

    Task<SignupResponse> SignupAsync(
        SignupRequest request,
        CancellationToken cancellationToken = default
    );
}
