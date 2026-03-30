using System;

namespace DateBoard.Models
{
    public enum NotificationType
    {
        Like,           // Кто-то лайкнул
        MutualLike,     // Взаимный лайк (Match)
        Message,        // Новое сообщение
        ProfileView,    // Кто-то посмотрел профиль
        SuperLike       // Суперлайк
    }

    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }           // Кому уведомление
        public string FromUserId { get; set; }     // От кого
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Навигация
        public Profile FromProfile { get; set; }
    }
}