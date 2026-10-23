using BaseAccountSystem.Server.DTOs.ActivityLogs;
using BaseAccountSystem.Server.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseAccountSystem.Server.Controllers
{
    public class ActivityLogController : AppBaseController
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllActivityLogs()
        {
            List<ActivityLogDto> logs = await _activityLogService.GetAllActivityLogs();

            return Ok(new { message = "Success", activityLogs = logs });
        }
    }
}
