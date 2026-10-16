using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.Interfaces.Services
{
    public interface ITokenService
    {
        public Task<string> CreateToken(Account account);
        public string CreateRefreshToken();
    }
}
