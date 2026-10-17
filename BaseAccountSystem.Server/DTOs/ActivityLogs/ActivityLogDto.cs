using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.DTOs.ActivityLogs
{
    public class ActivityLogDto
    {
        public string? UserId {  get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? ActivityType { get; set; }
        public bool IsSuccess { get; set; }
        public string? Details { get; set; }
        public string? UserClient { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? IpAddress { get; set; }
    }
}
