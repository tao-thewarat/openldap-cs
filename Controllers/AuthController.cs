using Microsoft.AspNetCore.Mvc;
using OpenLdapCs.DTOs;

namespace OpenLdapCs.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        return Ok(new LoginResponse { Message = "OK" });
    }
}
