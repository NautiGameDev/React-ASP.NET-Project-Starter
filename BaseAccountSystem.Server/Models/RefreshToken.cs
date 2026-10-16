using System.ComponentModel.DataAnnotations.Schema;

namespace BaseAccountSystem.Server.Models
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? TokenHash { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; } //Updated when token is terminated
        public string? ReplacedByToken { get; set; } //Maps to newest token

        public void Revoke()
        {
            Revoked = DateTime.UtcNow;
        }

        public void Replace(string newToken)
        {
            ReplacedByToken = newToken;
        }
    }
}
