using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace DateBoard.Hubs
{
    [Authorize]
    public class PrivateHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public PrivateHub(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task SendPrivateMessage(string receiverId, string text)
        {
            var sender = await _userManager.GetUserAsync(Context.User!);
            if (sender == null) return;

            var message = new PrivateMessage
            {
                SenderId = sender.Id,
                ReceiverId = receiverId,
                Text = text,
                SentAt = DateTime.UtcNow
            };
            _db.PrivateMessages.Add(message);
            await _db.SaveChangesAsync();

            var time = message.SentAt.ToString("HH:mm");
            await Clients.User(receiverId).SendAsync("ReceivePrivateMessage", sender.Id, text, time);
            await Clients.User(sender.Id).SendAsync("ReceivePrivateMessage", sender.Id, text, time);
        }
    }
}