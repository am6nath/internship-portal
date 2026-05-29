using System;
using System.Security.Claims;
using System.Threading.Tasks;
using InternshipPortal.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipPortal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET USER NOTIFICATIONS
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            
            return Ok(new
            {
                success = true,
                statusCode = 200,
                data = notifications
            });
        }

        // MARK SINGLE AS READ
        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(userId, id);
            
            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "Notification marked as read"
            });
        }

        // MARK ALL AS READ
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            
            return Ok(new
            {
                success = true,
                statusCode = 200,
                message = "All notifications marked as read"
            });
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId!);
        }
    }
}
