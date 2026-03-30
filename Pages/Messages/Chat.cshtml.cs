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
    public class ChatModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<PrivateMessage> Messages { get; set; } = new();
        public DateBoard.Models.Profile? OtherProfile { get; set; }
        public string OtherUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToPage("/Index");

            CurrentUserId = currentUser.Id;
            OtherUserId = id;
            OtherProfile = _db.Profiles.FirstOrDefault(p => p.UserId == id);

            Messages = await _db.PrivateMessages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == id) ||
                            (m.SenderId == id && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var unread = _db.PrivateMessages
                .Where(m => m.SenderId == id && m.ReceiverId == currentUser.Id && !m.IsRead);
            foreach (var msg in unread) msg.IsRead = true;
            await _db.SaveChangesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id, string text)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrWhiteSpace(text))
                return RedirectToPage(new { id });

            _db.PrivateMessages.Add(new PrivateMessage
            {
                SenderId = currentUser.Id,
                ReceiverId = id,
                Text = text,
                SentAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return RedirectToPage(new { id });
        }
    }
}