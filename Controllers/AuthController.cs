using Microsoft.AspNetCore.Mvc;
using OpenLdapCs.DTOs;
using OpenLdapCs.Interfaces;

namespace OpenLdapCs.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILdapAuthService ldapAuthService;

    public AuthController(ILdapAuthService ldapAuthService)
    {
        this.ldapAuthService = ldapAuthService;
    }

    [HttpPost("signin")]
    public async Task<IActionResult> Signin(
        [FromBody] SigninRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await ldapAuthService.SigninAsync(request, cancellationToken);
        return response.Success ? Ok(response) : Unauthorized(response);
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
}
