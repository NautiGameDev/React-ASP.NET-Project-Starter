using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.Interfaces.Repos
{
    public interface IAccountRepo
    {
        public Task CreateRefreshToken(RefreshToken token);
        public Task UpdateRefreshToken(RefreshToken token);
        public Task<List<RefreshToken>> GetAllRefreshTokensByUser(string userId);
        public Task<RefreshToken?> GetTokenByRawToken(string rawRefreshToken);
    }
}
