using DateBoard.Data;
using DateBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Services
{
    public interface IOnlineStatusService
    {
        Task UpdateActivity(string userId);
        Task<bool> IsOnline(string userId);
        Task<List<string>> GetOnlineUsers();
    }

    public class OnlineStatusService : IOnlineStatusService
    {
        private readonly ApplicationDbContext _context;

        public OnlineStatusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateActivity(string userId)
        {
            var status = await _context.OnlineStatuses
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (status == null)
            {
                status = new OnlineStatus { UserId = userId };
                _context.OnlineStatuses.Add(status);
            }

            status.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsOnline(string userId)
        {
            var status = await _context.OnlineStatuses
                .FirstOrDefaultAsync(o => o.UserId == userId);

            return status != null && (DateTime.UtcNow - status.LastActivity).TotalMinutes < 5;
        }

        public async Task<List<string>> GetOnlineUsers()
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            return await _context.OnlineStatuses
                .Where(o => o.LastActivity > fiveMinutesAgo)
                .Select(o => o.UserId)
                .ToListAsync();
        }
    }
}