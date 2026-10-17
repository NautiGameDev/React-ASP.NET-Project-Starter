using BaseAccountSystem.Server.DTOs.ActivityLogs;
using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.Helpers.Mappers
{
    public static class ActivityLogMapper
    {
        public static ActivityLogDto ToActivityLogDto(this ActivityLog activityLog)
        {
            return new ActivityLogDto()
            {
                UserId = activityLog.UserId,
                UserName = activityLog.Account.UserName ?? "Unknown",
                Email = activityLog.Account.Email ?? "unknown",
                ActivityType = activityLog.ActivityType.ToString(),
                IsSuccess = activityLog.IsSuccess,
                Details = activityLog.Details,
                UserClient = activityLog.UserClient,
                TimeStamp = activityLog.TimeStamp,
                IpAddress = activityLog.IpAddress
            };
        }
    }
}
