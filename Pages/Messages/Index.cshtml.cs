using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Pages.Messages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<DialogItem> Dialogs { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToPage("/Account/Login");

            // Получаем ID всех собеседников (отправлено ИЛИ получено)
            var sentTo = await _db.PrivateMessages
                .Where(m => m.SenderId == currentUser.Id)
                .Select(m => m.ReceiverId)
                .Distinct()
                .ToListAsync();

            var receivedFrom = await _db.PrivateMessages
                .Where(m => m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId)
                .Distinct()
                .ToListAsync();

            var allPartnerIds = sentTo.Union(receivedFrom).Distinct().ToList();

            foreach (var partnerId in allPartnerIds)
            {
                // Последнее сообщение в диалоге
                var lastMessage = await _db.PrivateMessages
                    .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == partnerId) ||
                               (m.SenderId == partnerId && m.ReceiverId == currentUser.Id))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                if (lastMessage == null) continue;

                // Профиль собеседника
                var partnerProfile = await _db.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == partnerId);

                // Подсчёт непрочитанных
                var unreadCount = await _db.PrivateMessages
                    .CountAsync(m => m.SenderId == partnerId &&
                                    m.ReceiverId == currentUser.Id &&
                                    !m.IsRead);

                Dialogs.Add(new DialogItem
                {
                    UserId = partnerId,
                    Name = partnerProfile?.Name ?? "Пользователь",
                    PhotoPath = partnerProfile?.PhotoPath,  // ← исправлено с ProfilePhotoPath
                    LastMessage = lastMessage.Text.StartsWith("[photo]") ? "Фото" : lastMessage.Text,
                    LastMessageTime = lastMessage.SentAt,
                    UnreadCount = unreadCount
                });
            }

            Dialogs = Dialogs.OrderByDescending(d => d.LastMessageTime).ToList();
            return Page();
        }
    }

    public class DialogItem
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}