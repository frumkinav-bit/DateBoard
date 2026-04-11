using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using DateBoard.Data;
using DateBoard.Models;

namespace DateBoard.Pages.Likes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public List<LikeViewModel> ReceivedLikes { get; set; } = new();
        public List<LikeViewModel> SentLikes { get; set; } = new();
        public List<LikeViewModel> Matches { get; set; } = new();
        public string ActiveTab { get; set; } = "received";

        public async Task<IActionResult> OnGetAsync(string tab = "received")
        {
            try
            {
                ActiveTab = tab;

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogWarning("User not authenticated");
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                _logger.LogInformation("Loading likes for user: {UserId}", currentUser.Id);

                var myProfileId = await _context.Profiles
                    .Where(p => p.UserId == currentUser.Id)
                    .Select(p => (int?)p.Id)
                    .FirstOrDefaultAsync();

                if (myProfileId == null)
                {
                    _logger.LogWarning("Profile not found for user: {UserId}", currentUser.Id);
                    // ✅ ИСПРАВЛЕНО: Показываем страницу с пустыми списками вместо редиректа
                    ReceivedLikes = new List<LikeViewModel>();
                    SentLikes = new List<LikeViewModel>();
                    Matches = new List<LikeViewModel>();
                    return Page();
                }

                // Кто меня лайкнул
                var receivedFavoriteIds = await _context.Favorites
                    .Where(f => f.ProfileId == myProfileId)
                    .Select(f => f.UserId)
                    .ToListAsync();

                _logger.LogInformation("Received {Count} favorites", receivedFavoriteIds.Count);

                ReceivedLikes = await _context.Profiles
                    .Where(p => receivedFavoriteIds.Contains(p.UserId))
                    .Select(p => new LikeViewModel
                    {
                        Profile = p,
                        IsMatch = _context.Favorites.Any(f => f.UserId == currentUser.Id && f.ProfileId == p.Id),
                        LikeDate = _context.Favorites
                            .Where(f => f.UserId == p.UserId && f.ProfileId == myProfileId)
                            .Select(f => (DateTime?)f.CreatedAt)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                // Кого я лайкнул
                var sentFavoriteIds = await _context.Favorites
                    .Where(f => f.UserId == currentUser.Id)
                    .Select(f => f.ProfileId)
                    .ToListAsync();

                SentLikes = await _context.Profiles
                    .Where(p => sentFavoriteIds.Contains(p.Id))
                    .Select(p => new LikeViewModel
                    {
                        Profile = p,
                        IsMatch = _context.Favorites.Any(f => f.UserId == p.UserId && f.ProfileId == myProfileId),
                        LikeDate = _context.Favorites
                            .Where(f => f.UserId == currentUser.Id && f.ProfileId == p.Id)
                            .Select(f => (DateTime?)f.CreatedAt)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                Matches = ReceivedLikes.Where(r => r.IsMatch).ToList();

                _logger.LogInformation("Loaded: Received={Received}, Sent={Sent}, Matches={Matches}",
                    ReceivedLikes.Count, SentLikes.Count, Matches.Count);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading likes page");
                throw;
            }
        }

        public class LikeViewModel
        {
            public DateBoard.Models.Profile Profile { get; set; } = null!;
            public bool IsMatch { get; set; }
            public DateTime? LikeDate { get; set; }
        }
    }
}