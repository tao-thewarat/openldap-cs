using OpenLdapCs.DTOs;
using OpenLdapCs.Interfaces;
using OpenLdapCs.Models;

namespace OpenLdapCs.Services;

public sealed class LdapAuthService : ILdapAuthService
{
    private readonly ILdapDirectoryService ldapDirectoryService;
    private readonly IRefreshTokenStore refreshTokenStore;
    private readonly ITokenService tokenService;

    public LdapAuthService(
        ILdapDirectoryService ldapDirectoryService,
        IRefreshTokenStore refreshTokenStore,
        ITokenService tokenService
    )
    {
        this.ldapDirectoryService = ldapDirectoryService;
        this.refreshTokenStore = refreshTokenStore;
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

        if (!isValid)
        {
            return new SigninResponse
            {
                Success = false,
                Message = "Invalid username or password.",
            };
        }

        var accessToken = tokenService.GenerateAccessToken(request.Username, distinguishedName);
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenExpiresAtUtc = tokenService.GetRefreshTokenExpirationUtc();

        await refreshTokenStore.StoreAsync(
            refreshToken,
            new RefreshTokenRecord
            {
                Username = request.Username,
                DistinguishedName = distinguishedName,
                ExpiresAtUtc = refreshTokenExpiresAtUtc,
            },
            refreshTokenExpiresAtUtc - DateTime.UtcNow,
            cancellationToken
        );

        return new SigninResponse
        {
            Success = true,
            Message = "Signin successful.",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    public async Task<SigninResponse> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var storedToken = await refreshTokenStore.GetAsync(refreshToken, cancellationToken);
        if (storedToken is null || storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return new SigninResponse
            {
                Success = false,
                Message = "Refresh token is invalid or expired.",
            };
        }

        await refreshTokenStore.RemoveAsync(refreshToken, cancellationToken);

        var newAccessToken = tokenService.GenerateAccessToken(
            storedToken.Username,
            storedToken.DistinguishedName
        );
        var newRefreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenExpiresAtUtc = tokenService.GetRefreshTokenExpirationUtc();

        await refreshTokenStore.StoreAsync(
            newRefreshToken,
            new RefreshTokenRecord
            {
                Username = storedToken.Username,
                DistinguishedName = storedToken.DistinguishedName,
                ExpiresAtUtc = refreshTokenExpiresAtUtc,
            },
            refreshTokenExpiresAtUtc - DateTime.UtcNow,
            cancellationToken
        );

        return new SigninResponse
        {
            Success = true,
            Message = "Token refreshed successfully.",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
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
