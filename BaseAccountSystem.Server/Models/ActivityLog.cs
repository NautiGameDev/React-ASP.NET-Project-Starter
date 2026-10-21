using System.ComponentModel.DataAnnotations.Schema;

namespace BaseAccountSystem.Server.Models
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Account))]
        public string? UserId { get; set; }
        public virtual Account? Account { get; set; }


        public ActivityTypes ActivityType { get; set; } = ActivityTypes.Undefined;
        public bool IsSuccess { get; set; } = true;
        public string? Details { get; set; }
        public string? UserClient { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? IpAddress { get; set; }
    }

    public enum ActivityTypes
    {
        Undefined = 0,
        Login = 1,
        Register = 2,
        Logout = 3,
        RefreshToken = 4,
        AccountUpdate = 5,
        PasswordResetRequest = 6,
        AccountLocked = 7
    }
}
