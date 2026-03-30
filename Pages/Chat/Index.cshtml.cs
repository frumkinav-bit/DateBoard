using DateBoard.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Pages.Chat
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

        public List<ChatMessage> Messages { get; set; } = new();

        public async Task OnGetAsync()
        {
            var messages = await _db.Messages
                .Include(m => m.User)
                .OrderBy(m => m.SentAt)
                .Take(100)
                .ToListAsync();

            Messages = messages.Select(m => new ChatMessage
            {
                UserId = m.UserId,
                UserName = m.User?.UserName ?? "Аноним",
                Text = m.Text,
                SentAt = m.SentAt
            }).ToList();
        }
    }

    public class ChatMessage
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
