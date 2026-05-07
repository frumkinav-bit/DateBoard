using DateBoard.Services;
using System.Security.Claims;

namespace DateBoard.Middleware
{
    public class OnlineStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public OnlineStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOnlineStatusService onlineService)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    // Обновляем каждые 30 секунд (чтобы не грузить БД)
                    await onlineService.UpdateActivity(userId);
                }
            }

            await _next(context);
        }
    }
}