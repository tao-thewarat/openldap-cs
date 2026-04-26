using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenLdapCs.DTOs;
using OpenLdapCs.Interfaces;
using OpenLdapCs.Options;

namespace OpenLdapCs.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILdapAuthService ldapAuthService;
    private readonly JwtOptions jwtOptions;

    public AuthController(ILdapAuthService ldapAuthService, IOptions<JwtOptions> jwtOptions)
    {
        this.ldapAuthService = ldapAuthService;
        this.jwtOptions = jwtOptions.Value;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var distinguishedName =
            User.FindFirstValue("dn") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(distinguishedName))
        {
            return Unauthorized(
                new SigninResponse
                {
                    Success = false,
                    Message = "User claims are missing from the access token.",
                }
            );
        }

        return Ok(new MeResponse { Username = username, DistinguishedName = distinguishedName });
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Signin(
        [FromBody] SigninRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await ldapAuthService.SigninAsync(request, cancellationToken);
        if (!response.Success)
        {
            return Unauthorized(response);
        }

        AppendAuthCookies(response);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(jwtOptions.RefreshTokenCookieName, out var refreshToken))
        {
            return Unauthorized(
                new SigninResponse { Success = false, Message = "Refresh token cookie is missing." }
            );
        }

        var response = await ldapAuthService.RefreshAsync(refreshToken, cancellationToken);
        if (!response.Success)
        {
            DeleteAuthCookies();
            return Unauthorized(response);
        }

        AppendAuthCookies(response);
        return Ok(response);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await ldapAuthService.SignupAsync(request, cancellationToken);
        return response.Success ? Ok(response) : Conflict(response);
    }

    private void AppendAuthCookies(SigninResponse response)
    {
        Response.Cookies.Append(
            jwtOptions.AccessTokenCookieName,
            response.AccessToken!,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes),
            }
        );

        Response.Cookies.Append(
            jwtOptions.RefreshTokenCookieName,
            response.RefreshToken!,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshTokenExpirationDays),
            }
        );
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete(jwtOptions.AccessTokenCookieName);
        Response.Cookies.Delete(jwtOptions.RefreshTokenCookieName);
    }
}
