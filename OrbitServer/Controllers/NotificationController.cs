// File Name: NotificationController.cs
// Description: Handles all notification related business logic

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Get all notifications by user id
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetNotificationsByUserId(string id)
    {
        var notifications = await _notificationService.GetNotificationsByUserIdAsync(id);
        if (notifications == null)
        {
            return NotFound();
        }
        return Ok(notifications);
    }

    // Get notification by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(string id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }
        return Ok(notification);
    }

    // Get all unseen notifications by user id
    [HttpGet("user/unseen/{id}")]
    public async Task<IActionResult> GetUnseenNotificationsByUserId(string id)
    {
        var notifications = await _notificationService.GetUnseenNotificationsByUserIdAsync(id);
        if (notifications == null)
        {
            return NotFound();
        }
        return Ok(notifications);
    }

    // Update notification seen status
    [HttpPut("seen/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromQuery, Required] bool seenStatus)
    {
        var existingNotification = await _notificationService.GetNotificationByIdAsync(id);
        if (existingNotification == null)
        {
            return NotFound();
        }

        existingNotification.SeenStatus = seenStatus;

        existingNotification.UpdatedAt = DateTime.Now;
        await _notificationService.UpdateNotificationAsync(id, existingNotification);

        return Ok(existingNotification);
    }

    // Delete notification by id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        var existingNotification = await _notificationService.GetNotificationByIdAsync(id);
        if (existingNotification == null)
        {
            return NotFound();
        }

        await _notificationService.DeleteNotificationAsync(id);
        return Ok($"Successfully deleted notification with id: {id}");
    }
}
