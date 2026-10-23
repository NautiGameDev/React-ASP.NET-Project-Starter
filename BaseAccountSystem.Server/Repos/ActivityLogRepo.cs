using System.Net;
using BaseAccountSystem.Server.Data;
using BaseAccountSystem.Server.Interfaces.Repos;
using BaseAccountSystem.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseAccountSystem.Server.Repos
{
    public class ActivityLogRepo : IActivityLogRepo
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityLog>> GetActivityLogsByClient(string client)
        {
            return await _context.ActivityLogs
                .Where(al => al.UserClient == client)
                .Include(al => al.Account)
                .OrderByDescending(al => al.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetActivityLogsByIp(string ipAddress)
        {
            return await _context.ActivityLogs
                .Where(al => al.IpAddress == ipAddress)
                .Include(al => al.Account)
                .OrderByDescending(al => al.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetActivityLogsByUserId(string userId)
        {
            return await _context.ActivityLogs
                .Where(al => al.UserId == userId)
                .Include(al => al.Account)
                .OrderByDescending(al => al.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetAllActivityLogs()
        {
            return await _context.ActivityLogs
                .Include(al => al.Account)
                .ToListAsync();
        }

        public async Task PostActivityLog(ActivityLog activityLog)
        {
            await _context.ActivityLogs.AddAsync(activityLog);
            await _context.SaveChangesAsync();
        }
    }
}
