using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DateBoard.Services;
using System.Security.Claims;

namespace DateBoard.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetUserNotifications(userId);
            return Ok(notifications);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _notificationService.GetUnreadCount(userId);
            return Ok(count);
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsRead(id);
            return Ok();
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notificationService.MarkAllAsRead(userId);
            return Ok();
        }
    }
}