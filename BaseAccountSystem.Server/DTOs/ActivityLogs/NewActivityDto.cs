using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.DTOs.ActivityLogs
{
    public class NewActivityDto
    {
        public string? UserId { get; set; }
        public ActivityTypes ActivityType { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string? Details { get; set; } = "";
        public string? UserClient { get; set; } = "";
        public string? IpAddress { get; set; } = "";
    }
}
