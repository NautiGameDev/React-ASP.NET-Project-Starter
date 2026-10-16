using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BaseAccountSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppBaseController : ControllerBase
    {
        protected string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
