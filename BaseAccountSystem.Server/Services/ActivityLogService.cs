using BaseAccountSystem.Server.DTOs.ActivityLogs;
using BaseAccountSystem.Server.Helpers.Mappers;
using BaseAccountSystem.Server.Interfaces.Repos;
using BaseAccountSystem.Server.Interfaces.Services;
using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepo _activityLogRepo;

        public ActivityLogService(IActivityLogRepo activityLogRepo)
        {
            _activityLogRepo = activityLogRepo;
        }

        public async Task CreateActivityLog(string userId, ActivityTypes activityType, bool isSuccess, string details, string userClient, string ip)
        {
            ActivityLog newLog = new ActivityLog()
            {
                UserId = userId,
                ActivityType = activityType,
                IsSuccess = isSuccess,
                Details = details,
                UserClient = userClient,
                TimeStamp = DateTime.UtcNow,
                IpAddress = ip
            };

            await _activityLogRepo.PostActivityLog(newLog);
        }

        public async Task<List<ActivityLogDto>> GetActivityLogsByClient(string client)
        {
            List<ActivityLog> logs = await _activityLogRepo.GetActivityLogsByClient(client);

            return logs.Select(l => l.ToActivityLogDto()).ToList();
        }

        public async Task<List<ActivityLogDto>> GetActivityLogsByIp(string ipAddress)
        {
            List<ActivityLog> logs = await _activityLogRepo.GetActivityLogsByIp(ipAddress);

            return logs.Select(l => l.ToActivityLogDto()).ToList();
        }

        public async Task<List<ActivityLogDto>> GetActivityLogsByUserId(string userId)
        {
            List<ActivityLog> logs = await _activityLogRepo.GetActivityLogsByUserId(userId);

            return logs.Select(l => l.ToActivityLogDto()).ToList();
        }
    }
}
