using BaseAccountSystem.Server.DTOs.ActivityLogs;
using BaseAccountSystem.Server.Models;

namespace BaseAccountSystem.Server.Interfaces.Services
{
    public interface IActivityLogService
    {
        public Task CreateActivityLog(string? userId, ActivityTypes activityType, bool isSuccess, string details, string userClient, string ip);
        public Task<List<ActivityLogDto>> GetActivityLogsByUserId(string userId);
        public Task<List<ActivityLogDto>> GetActivityLogsByIp(string ipAddress);
        public Task<List<ActivityLogDto>> GetActivityLogsByClient(string client);
        public Task<List<ActivityLogDto>> GetAllActivityLogs();

    }
}
