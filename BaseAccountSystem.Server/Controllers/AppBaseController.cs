using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BaseAccountSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppBaseController : ControllerBase
    {
        protected string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
        protected string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        protected string? UserClient => Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}
