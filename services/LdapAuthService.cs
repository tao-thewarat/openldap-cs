using OpenLdapCs.DTOs;
using OpenLdapCs.Interfaces;

namespace OpenLdapCs.Services;

public sealed class LdapAuthService : ILdapAuthService
{
    private readonly ILdapDirectoryService ldapDirectoryService;
    private readonly ITokenService tokenService;

    public LdapAuthService(ILdapDirectoryService ldapDirectoryService, ITokenService tokenService)
    {
        this.ldapDirectoryService = ldapDirectoryService;
        this.tokenService = tokenService;
    }

    public async Task<SigninResponse> SigninAsync(
        SigninRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var distinguishedName = await ldapDirectoryService.FindUserDnAsync(
            request.Username,
            cancellationToken
        );

        if (string.IsNullOrWhiteSpace(distinguishedName))
        {
            return new SigninResponse
            {
                Success = false,
                Message = "User not found.",
                AccessToken = null,
                RefreshToken = null,
            };
        }

        var isValid = await ldapDirectoryService.ValidateCredentialsAsync(
            distinguishedName,
            request.Password,
            cancellationToken
        );

        return new SigninResponse
        {
            Success = isValid,
            Message = isValid ? "Signin successful." : "Invalid username or password.",
            AccessToken = isValid
                ? tokenService.GenerateAccessToken(request.Username, distinguishedName)
                : null,
            RefreshToken = isValid ? tokenService.GenerateRefreshToken() : null,
        };
    }

    public async Task<SignupResponse> SignupAsync(
        SignupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var userExists = await ldapDirectoryService.UserExistsAsync(
            request.Username,
            cancellationToken
        );

        if (userExists)
        {
            return new SignupResponse { Success = false, Message = "User already exists." };
        }

        var distinguishedName = await ldapDirectoryService.CreateUserAsync(
            request.Username,
            request.Password,
            cancellationToken
        );

        return new SignupResponse
        {
            Success = true,
            Message = "User created successfully.",
            DistinguishedName = distinguishedName,
        };
    }
}
