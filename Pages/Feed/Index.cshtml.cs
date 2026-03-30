using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DateBoard.Data;
using DateBoard.Models;
using DateBoard.Services;
using DateBoard.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DateBoard.Pages.Feed
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOnlineStatusService _onlineStatusService;

        public IndexModel(
            ApplicationDbContext context,
            INotificationService notificationService,
            IHubContext<NotificationHub> notificationHub,
            UserManager<IdentityUser> userManager,
            IOnlineStatusService onlineStatusService)
        {
            _context = context;
            _notificationService = notificationService;
            _notificationHub = notificationHub;
            _userManager = userManager;
            _onlineStatusService = onlineStatusService;
        }

        public List<ProfileViewModel> Profiles { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string City { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Goal { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinAge { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MaxAge { get; set; }

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var likedProfileIds = await _context.Favorites
                .Where(f => f.UserId == currentUser.Id)
                .Select(f => f.ProfileId)
                .ToListAsync();

            var query = _context.Profiles
                .Where(p => p.UserId != currentUser.Id)
                .Where(p => !likedProfileIds.Contains(p.Id))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(City))
            {
                query = query.Where(p => p.City.Contains(City));
            }

            if (!string.IsNullOrWhiteSpace(Goal))
            {
                query = query.Where(p => p.Goal == Goal);
            }

            if (MinAge.HasValue)
            {
                query = query.Where(p => p.Age >= MinAge.Value);
            }

            if (MaxAge.HasValue)
            {
                query = query.Where(p => p.Age <= MaxAge.Value);
            }

            var profiles = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var onlineUsers = await _onlineStatusService.GetOnlineUsers();

            Profiles = profiles.Select(p => new ProfileViewModel
            {
                Profile = p,
                IsOnline = onlineUsers.Contains(p.UserId)
            }).ToList();
        }

        public async Task<IActionResult> OnPostLike(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null) return NotFound();

            var existingLike = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.ProfileId == id);

            if (existingLike == null)
            {
                var favorite = new Favorite
                {
                    UserId = currentUser.Id,
                    ProfileId = id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();

                var myProfile = await _context.Profiles
                    .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

                await _notificationService.CreateNotification(
                    profile.UserId,
                    currentUser.Id,
                    NotificationType.Like,
                    $"{myProfile?.Name ?? "Кто-то"} лайкнул(а) вашу анкету"
                );

                await NotificationHub.SendNotification(
                    _notificationHub,
                    profile.UserId,
                    "Like",
                    $"{myProfile?.Name ?? "Кто-то"} лайкнул(а) вашу анкету",
                    currentUser.Id,
                    myProfile?.Name ?? "Кто-то",
                    myProfile?.PhotoPath ?? ""
                );

                var myProfileId = await _context.Profiles
                    .Where(p => p.UserId == currentUser.Id)
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync();

                var mutualLike = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == profile.UserId && f.ProfileId == myProfileId);

                if (mutualLike != null)
                {
                    await _notificationService.CreateNotification(
                        profile.UserId,
                        currentUser.Id,
                        NotificationType.MutualLike,
                        $"У вас взаимная симпатия с {myProfile?.Name}!"
                    );

                    await NotificationHub.SendNotification(
                        _notificationHub,
                        profile.UserId,
                        "MutualLike",
                        $"У вас взаимная симпатия! Начните общение",
                        currentUser.Id,
                        myProfile?.Name ?? "Кто-то",
                        myProfile?.PhotoPath ?? ""
                    );

                    await _notificationService.CreateNotification(
                        currentUser.Id,
                        profile.UserId,
                        NotificationType.MutualLike,
                        $"У вас взаимная симпатия с {profile.Name}!"
                    );

                    await NotificationHub.SendNotification(
                        _notificationHub,
                        currentUser.Id,
                        "MutualLike",
                        $"У вас взаимная симпатия с {profile.Name}!",
                        profile.UserId,
                        profile.Name,
                        profile.PhotoPath ?? ""
                    );
                }
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostSuperLike(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null) return NotFound();

            var myProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == currentUser.Id);

            var existingLike = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.ProfileId == id);

            if (existingLike == null)
            {
                var favorite = new Favorite
                {
                    UserId = currentUser.Id,
                    ProfileId = id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Favorites.Add(favorite);
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotification(
                profile.UserId,
                currentUser.Id,
                NotificationType.SuperLike,
                $"⭐ {myProfile?.Name ?? "Кто-то"} поставил(а) вам суперлайк!"
            );

            await NotificationHub.SendNotification(
                _notificationHub,
                profile.UserId,
                "SuperLike",
                $"Поставил(а) вам суперлайк!",
                currentUser.Id,
                myProfile?.Name ?? "Кто-то",
                myProfile?.PhotoPath ?? ""
            );

            var myProfileId = await _context.Profiles
                .Where(p => p.UserId == currentUser.Id)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            var mutualLike = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == profile.UserId && f.ProfileId == myProfileId);

            if (mutualLike != null)
            {
                await _notificationService.CreateNotification(
                    profile.UserId,
                    currentUser.Id,
                    NotificationType.MutualLike,
                    $"У вас взаимная симпатия с {myProfile?.Name}!"
                );

                await NotificationHub.SendNotification(
                    _notificationHub,
                    profile.UserId,
                    "MutualLike",
                    $"У вас взаимная симпатия! Начните общение",
                    currentUser.Id,
                    myProfile?.Name ?? "Кто-то",
                    myProfile?.PhotoPath ?? ""
                );
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostPass(int id)
        {
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostUnlike(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.ProfileId == id);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { success = true });
        }

        public class ProfileViewModel
        {
            public DateBoard.Models.Profile Profile { get; set; }
            public bool IsOnline { get; set; }
        }
    }
}