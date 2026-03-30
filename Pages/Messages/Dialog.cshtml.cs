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
    public class DialogModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DialogModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<PrivateMessage> Messages { get; set; } = new();
        public DateBoard.Models.Profile? OtherProfile { get; set; }
        public string OtherUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
        public string OtherInitial { get; set; } = "";  // Добавляем для View

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToPage("/Index");

            CurrentUserId = currentUser.Id;
            OtherUserId = userId;

            // Загружаем профиль оппонента
            OtherProfile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            OtherInitial = OtherProfile?.Name?.Substring(0, 1).ToUpper() ?? "U";

            // Загружаем сообщения с Include для Sender/Receiver
            Messages = await _db.PrivateMessages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Помечаем прочитанными
            var unread = await _db.PrivateMessages
                .Where(m => m.SenderId == userId && m.ReceiverId == currentUser.Id && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unread)
                msg.IsRead = true;

            await _db.SaveChangesAsync();

            return Page();
        }
    }
}