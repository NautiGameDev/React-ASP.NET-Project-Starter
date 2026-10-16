using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.DTOs.API;
using BaseAccountSystem.Server.Helpers;
using BaseAccountSystem.Server.Interfaces.Services;
using BaseAccountSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseAccountSystem.Server.Controllers
{
    public class AccountController : AppBaseController
    {
        private readonly IAccountService _accountService;
        private readonly SignInManager<Account> _signInManager;
        private readonly IConfiguration _config;

        public AccountController(IAccountService accountService, SignInManager<Account> signInManager, IConfiguration config)
        {
            _accountService = accountService;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            AuthResultDto result = await _accountService.Login(dto, ipAddress);

            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    AuthErrorType.NotFound or AuthErrorType.InvalidCredentials => Unauthorized(new { message = result.ErrorMessage }),
                    _ => BadRequest(new { message = result.ErrorMessage })
                };
            }

            SetCookie(result.Token, result.RefreshToken);

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] NewAccountDto dto)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            AuthResultDto result = await _accountService.Register(dto, ipAddress);

            if (!result.Success)
            {
                return Conflict(new { message = result.ErrorMessage });
            }


            SetCookie(result.Token, result.RefreshToken);

            return StatusCode(StatusCodes.Status201Created, new { message = "Account registration successful" });
        }

        [HttpGet("check-logged")]
        [Authorize]
        public async Task<IActionResult> CheckLoggedIn()
        {
            return Ok(new { isLogged = true });
        }

        [HttpGet("check-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckAdmin()
        {
            return Ok(new { isAdmin = true });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            string refreshCookieKey = _config["JWT:RefreshCookieName"] ?? "BASRefreshAuth";
            var refreshToken = Request.Cookies[refreshCookieKey];

            if (refreshToken != null)
            {
                await _accountService.RevokeToken(refreshToken);
            }            

            await ClearAuthCookie();

            return Ok(new { message = "logged out successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewToken()
        {
            
            string refreshCookieKey = _config["JWT:RefreshCookieName"] ?? "BASRefreshAuth";
            var refreshToken = Request.Cookies[refreshCookieKey];
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "Refresh token could not be found" });
            }

            AuthResultDto result = await _accountService.RefreshToken(refreshToken, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            SetCookie(result.Token, result.RefreshToken);

            return Ok(new { message = "Token created successfully" });
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateAccountDto dto)
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                return Unauthorized(new { message = "Failed to authorize account " });
            }

            AuthResultDto result = await _accountService.UpdateAccount(UserId, dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Account successfully updated" });
        }

        
        private void SetCookie(string token, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = TokenHandler.GetAuthTokenExpiration()
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = TokenHandler.GetRefreshTokenExpiration()
            };

            string authCookieKey = _config["JWT:CookieName"] ?? "BASAuth";
            string refreshCookieKey = _config["JWT:RefreshCookieName"] ?? "BASRefreshAuth";

            Response.Cookies.Append(authCookieKey, token, cookieOptions);
            Response.Cookies.Append(refreshCookieKey, refreshToken, refreshCookieOptions);
        }

        private async Task ClearAuthCookie()
        {
            await _signInManager.SignOutAsync();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            };

            string authCookieKey = _config["JWT:CookieName"] ?? "BASAuth";
            string refreshCookieKey = _config["JWT:RefreshCookieName"] ?? "BASRefreshAuth";

            Response.Cookies.Append(authCookieKey, "", cookieOptions);
            Response.Cookies.Append(refreshCookieKey, "", cookieOptions);
        }
    }
}
