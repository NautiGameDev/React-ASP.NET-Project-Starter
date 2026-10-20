using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.DTOs.ActivityLogs;
using BaseAccountSystem.Server.DTOs.API;
using BaseAccountSystem.Server.DTOs.Email;
using BaseAccountSystem.Server.Helpers;
using BaseAccountSystem.Server.Interfaces.Services;
using BaseAccountSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BaseAccountSystem.Server.Controllers
{
    public class AccountController : AppBaseController
    {
        private readonly IAccountService _accountService;
        private readonly SignInManager<Account> _signInManager;
        private readonly IConfiguration _config;
        private readonly IActivityLogService _activityLogService;

        public AccountController(IAccountService accountService, SignInManager<Account> signInManager, IConfiguration config, IActivityLogService activityLogService)
        {
            _accountService = accountService;
            _signInManager = signInManager;
            _config = config;
            _activityLogService = activityLogService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            AuthResultDto result = await _accountService.Login(dto, IpAddress);

            if (!result.Success)
            {
                await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.Login, false, result.ErrorMessage, UserClient, IpAddress);

                return result.ErrorType switch
                {
                    AuthErrorType.NotFound or AuthErrorType.InvalidCredentials => Unauthorized(new { message = result.ErrorMessage }),
                    _ => BadRequest(new { message = result.ErrorMessage })
                };
            }

            SetCookie(result.Token, result.RefreshToken);

            await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.Login, true, "User login successful", UserClient, IpAddress);

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] NewAccountDto dto)
        {
            AuthResultDto result = await _accountService.Register(dto, IpAddress);

            if (!result.Success)
            {
                await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.Register, false, result.ErrorMessage, UserClient, IpAddress);

                return Conflict(new { message = result.ErrorMessage });
            }                        

            await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.Register, true, "Account registration successful", UserClient, IpAddress);

            return StatusCode(StatusCodes.Status201Created, new { message = "Account registration successful" });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            AuthResultDto result = await _accountService.ConfirmAccount(dto);

            if (!result.Success)
            {
                await _activityLogService.CreateActivityLog(result.UserId ?? "Unknown", ActivityTypes.Register, false, "Failed to confirm account activation code", UserClient, IpAddress);

                return result.ErrorType switch
                {
                    AuthErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                    _ => BadRequest(new {message = result.ErrorMessage})
                };
            }

            await _activityLogService.CreateActivityLog(result.UserId ?? "Unknown", ActivityTypes.Register, true, "Account confirmation successful", UserClient, IpAddress);

            return Ok(new { message = "Account confirmed successfully" });
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

            await _activityLogService.CreateActivityLog(UserId, ActivityTypes.Logout, true, "Logged out successfully", UserClient, IpAddress);

            return Ok(new { message = "logged out successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewToken()
        {
            
            string refreshCookieKey = _config["JWT:RefreshCookieName"] ?? "BASRefreshAuth";
            var refreshToken = Request.Cookies[refreshCookieKey];

            if (string.IsNullOrEmpty(refreshToken))
            {               
                await _activityLogService.CreateActivityLog(UserId, ActivityTypes.RefreshToken, false, "Refresh token could not be found", UserClient, IpAddress);

                return Unauthorized(new { message = "Refresh token could not be found" });
            }

            AuthResultDto result = await _accountService.RefreshToken(refreshToken, IpAddress);

            if (!result.Success)
            {
                await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.RefreshToken, false, result.ErrorMessage, UserClient, IpAddress);

                return Unauthorized(new { message = result.ErrorMessage });
            }

            SetCookie(result.Token, result.RefreshToken);

            await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.RefreshToken, true, "New auth token successfully created", UserClient, IpAddress);

            return Ok(new { message = "New auth token successfully created" });
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateAccountDto dto)
        {
            if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                await _activityLogService.CreateActivityLog(UserId, ActivityTypes.AccountUpdate, false, "UserId or Password blank", UserClient, IpAddress);

                return Unauthorized(new { message = "Failed to authorize account" });
            }

            AuthResultDto result = await _accountService.UpdateAccount(UserId, dto);

            if (!result.Success)
            {   
                await _activityLogService.CreateActivityLog(UserId, ActivityTypes.AccountUpdate, false, result.ErrorMessage, UserClient, IpAddress);

                return BadRequest(new { message = result.ErrorMessage });
            }

            await _activityLogService.CreateActivityLog(result.UserId, ActivityTypes.AccountUpdate, true, "Account updated successfully", UserClient, IpAddress);

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
