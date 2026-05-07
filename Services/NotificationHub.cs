using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace DateBoard.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }

        // Метод для отправки уведомления конкретному пользователю
        public static async Task SendNotification(IHubContext<NotificationHub> hubContext,
            string userId, string type, string message, string fromUserId,
            string fromName, string fromPhoto)
        {
            await hubContext.Clients.Group(userId)
                .SendAsync("ReceiveNotification", type, message, fromUserId, fromName, fromPhoto);
        }
    }
}