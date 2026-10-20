using BaseAccountSystem.Server.DTOs.Account;
using BaseAccountSystem.Server.DTOs.API;
using BaseAccountSystem.Server.DTOs.Email;
using BaseAccountSystem.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace BaseAccountSystem.Server.Interfaces.Services
{
    public interface IAccountService
    {
        public Task<AuthResultDto> Login(LoginDto dto, string ip);
        public Task<AuthResultDto> Register(NewAccountDto dto, string ip);
        public Task<AuthResultDto> UpdateAccount(string userId, UpdateAccountDto dto);
        public Task<AuthResultDto> RefreshToken(string rawRefreshToken, string ip);
        public Task RevokeToken(string refreshToken);
        public Task<AuthResultDto> ConfirmAccount(ConfirmEmailDto dto);
    }
}
