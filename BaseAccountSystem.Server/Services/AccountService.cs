using System.Text;
using BaseAccountSystem.Server.Data;
using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.DTOs.API;
using BaseAccountSystem.Server.DTOs.Email;
using BaseAccountSystem.Server.Helpers;
using BaseAccountSystem.Server.Interfaces.Repos;
using BaseAccountSystem.Server.Interfaces.Services;
using BaseAccountSystem.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace BaseAccountSystem.Server.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<Account> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<Account> _signInManager;
        private readonly IAccountRepo _accountRepo;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;


        public AccountService(UserManager<Account> userManager, ITokenService tokenService, SignInManager<Account> signInManager, IAccountRepo accountRepo, ApplicationDbContext context, IConfiguration config, IEmailService emailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _accountRepo = accountRepo;
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        public async Task<AuthResultDto> Login(LoginDto dto, string ip)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Missing credentials",
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = "Unknown"
                };
            }

            var account = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == dto.UserName);

            if (account == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = $"Invalid Username/Password combination",
                    ErrorType = AuthErrorType.NotFound,
                    UserId = "Unknown"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(account, dto.Password, false);

            
            if (!result.Succeeded)
            {
                if (result.IsNotAllowed)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ErrorMessage = "Attempted login to unconfirmed account",
                        ErrorType = AuthErrorType.InvalidCredentials,
                        UserId = account.Id
                    };
                }
                else if (result.IsLockedOut)
                {
                    return new AuthResultDto()
                    {
                        Success = false,
                        ErrorMessage = "Attempted login to locked account",
                        ErrorType = AuthErrorType.InvalidCredentials,
                        UserId = account.Id
                    };
                }

                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = "Invalid Username/Password combination",
                    ErrorType = AuthErrorType.InvalidCredentials,
                    UserId = account.Id
                };
            }

            string token = await _tokenService.CreateToken(account);
            string refreshToken = await GenerateAndSaveRefreshToken(account.Id, ip);

            return new AuthResultDto()
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ErrorType = AuthErrorType.None,
                UserId = account.Id
            };
        }

        public async Task<AuthResultDto> Register(NewAccountDto dto, string ip)
        {
            if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = "Credentials not present in DTO",
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = "Unknown"
                };
            }

            var account = new Account
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var createdAccount = await _userManager.CreateAsync(account, dto.Password);

            if (!createdAccount.Succeeded)
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", createdAccount.Errors.Select(e => e.Description)),
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = "Unknown"
                };
            }

            //Handle confirmation code and email
            string rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(account);
            string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
            string confirmLink = $"{_config["ClientSettings:BaseUrl"]}/confirm-email?userId={account.Id}&code={encodedToken}";

            await _emailService.CreateEmail(new NewEmailDto()
            {
                ToEmail = account.Email,
                ToUser = account.UserName,
                Subject = "Confirm your account",
                HtmlBody = $"""
                    <p>Hello {account.UserName},</p>

                    <p>Please follow this link to finish account registration and confirm your account: <a href='{confirmLink}'>Account Confirmation</a></p>
                """
            });

            
            await _userManager.AddToRoleAsync(account, "User");                   

            return new AuthResultDto()
            {
                Success = true,
                Token = null,
                RefreshToken = null,
                ErrorType = AuthErrorType.None,
                UserId = account.Id
            };
        }
        
        public async Task<AuthResultDto> UpdateAccount(string userId, UpdateAccountDto dto)
        {
            Account? account = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (account == null)
            {
                return new AuthResultDto() 
                { 
                    Success = false,
                    ErrorMessage = "Account could not be found with that UserId",
                    ErrorType = AuthErrorType.NotFound,
                    UserId = "Unknown"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(account, dto.CurrentPassword, false);

            if (!result.Succeeded)
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = "Current password is incorrect",
                    ErrorType = AuthErrorType.InvalidCredentials,
                    UserId = account.Id
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    var passwordResult = await _userManager.ChangePasswordAsync(account, dto.CurrentPassword, dto.NewPassword);

                    if (!passwordResult.Succeeded)
                    {
                        var error = passwordResult.Errors.FirstOrDefault()?.Description ?? "Password update failed.";
                        throw new Exception(error ?? "Error updating email");
                    }
                }

                if (!string.IsNullOrWhiteSpace(dto.Email) && account.Email != dto.Email)
                {
                    var emailResult = await _userManager.SetEmailAsync(account, dto.Email);

                    if (!emailResult.Succeeded)
                    {
                        var error = emailResult.Errors.FirstOrDefault().Description ?? "Email update failed";
                        throw new Exception(error ?? "Error updating email");
                    }
                }

                await transaction.CommitAsync();

                return new AuthResultDto()
                {
                    Success = true,
                    ErrorType = AuthErrorType.None,
                    UserId = account.Id
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = account.Id
                };
            }            
        }

        public async Task<AuthResultDto> RefreshToken(string rawRefreshToken, string ip)
        {
            //Verify refresh token match
            RefreshToken? currentToken = await _accountRepo.GetTokenByRawToken(rawRefreshToken);

            if (currentToken == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Refresh token not found in database",
                    ErrorType = AuthErrorType.NotFound,
                    UserId = "Unknown"
                };
            }

            //Test if token has been revoked/replaced
            if (currentToken.Revoked != null || !string.IsNullOrEmpty(currentToken.ReplacedByToken))
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid refresh token session",
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = currentToken.UserId
                };
            }

            //Test if token is expired
            if (currentToken.Expires < DateTime.UtcNow)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Refresh token expired",
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = currentToken.UserId
                };
            }


            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //Generate new refresh token
                string newRefreshToken = _tokenService.CreateRefreshToken();

                RefreshToken newTokenObject = new RefreshToken()
                {
                    UserId = currentToken.UserId,
                    TokenHash = TokenHasher.HashToken(newRefreshToken),
                    Expires = TokenHandler.GetRefreshTokenExpiration(),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ip
                };                              

                await _accountRepo.CreateRefreshToken(newTokenObject);

                //Update current token
                currentToken.Revoke();
                currentToken.Replace(TokenHasher.HashToken(newRefreshToken));

                await _accountRepo.UpdateRefreshToken(currentToken);

                //Generate new auth token
                Account? account = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == currentToken.UserId);

                if (account == null)
                {
                    throw new Exception("Account could not be found in database");
                }

                string newAuthToken = await _tokenService.CreateToken(account);

                await transaction.CommitAsync();

                return new AuthResultDto
                {
                    Success = true,
                    ErrorType = AuthErrorType.None,
                    Token = newAuthToken,
                    RefreshToken = newRefreshToken,
                    UserId= currentToken.UserId
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred while processing session data",
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = currentToken.UserId
                };
            }            
        }
    
        private async Task<string> GenerateAndSaveRefreshToken(string userId, string ip)
        {
            string refreshToken = _tokenService.CreateRefreshToken();

            RefreshToken newRefreshToken = new RefreshToken()
            {
                UserId = userId,
                TokenHash = TokenHasher.HashToken(refreshToken),
                Expires = TokenHandler.GetRefreshTokenExpiration(),
                Created = DateTime.UtcNow,
                CreatedByIp = ip
            };

            await _accountRepo.CreateRefreshToken(newRefreshToken);

            return refreshToken;
        }

        public async Task RevokeToken(string refreshToken)
        {
            RefreshToken? currentToken = await _accountRepo.GetTokenByRawToken(refreshToken);

            if (currentToken == null)
            {
                return;
            }

            currentToken.Revoke();

            await _accountRepo.UpdateRefreshToken(currentToken);
        }

        public async Task<AuthResultDto> ConfirmAccount(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = "User could not be found by that ID",
                    ErrorType = AuthErrorType.NotFound,
                    UserId = dto.UserId
                };
            }

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.ConfirmationCode));
            }
            catch (FormatException)
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = "Invalid confirmation token format.",
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = user.Id
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return new AuthResultDto()
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)),
                    ErrorType = AuthErrorType.ProcessFailed,
                    UserId = user.Id
                };
            }

            return new AuthResultDto()
            {
                Success = true,
                ErrorType = AuthErrorType.None,
                UserId = user.Id               
            };
        }
    }
}
