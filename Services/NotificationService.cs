using DateBoard.Data;
using DateBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Services
{
    public interface INotificationService
    {
        Task CreateNotification(string userId, string fromUserId, NotificationType type, string message = null);
        Task<List<Notification>> GetUserNotifications(string userId, bool unreadOnly = false);
        Task MarkAsRead(int notificationId);
        Task MarkAllAsRead(string userId);
        Task<int> GetUnreadCount(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotification(string userId, string fromUserId, NotificationType type, string message = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                FromUserId = fromUserId,
                Type = type,
                Message = message ?? GetDefaultMessage(type),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotifications(string userId, bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Include(n => n.FromProfile)
                .Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsRead(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        private string GetDefaultMessage(NotificationType type) => type switch
        {
            NotificationType.Like => "Кому-то понравилась ваша анкета",
            NotificationType.MutualLike => "У вас взаимная симпатия!",
            NotificationType.Message => "Новое сообщение",
            NotificationType.ProfileView => "Кто-то посмотрел ваш профиль",
            NotificationType.SuperLike => "Вам поставили суперлайк!",
            _ => "Новое уведомление"
        };
    }
}