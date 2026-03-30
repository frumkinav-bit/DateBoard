using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatHub(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task SendMessage(string text)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == user!.Id);
            var displayName = profile?.Name ?? user?.Email?.Split('@')[0] ?? "Аноним";

            var message = new Message
            {
                UserId = user!.Id,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveMessage", displayName, text, message.SentAt.ToString("HH:mm"));
        }
    }
}