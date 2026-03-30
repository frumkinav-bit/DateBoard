using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DateBoard.Data;
using DateBoard.Models;

namespace DateBoard.Pages.Likes
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Кто меня лайкнул (с профилями)
        public List<LikeViewModel> ReceivedLikes { get; set; } = new();

        // Кого я лайкнул
        public List<LikeViewModel> SentLikes { get; set; } = new();

        // Взаимные лайки (Matches)
        public List<LikeViewModel> Matches { get; set; } = new();

        public string ActiveTab { get; set; } = "received";

        public async Task OnGetAsync(string tab = "received")
        {
            ActiveTab = tab;
            var currentUser = await _userManager.GetUserAsync(User);
            var myProfileId = await _context.Profiles
                .Where(p => p.UserId == currentUser.Id)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            // Кто меня лайкнул (люди, у которых в избранном мой профиль)
            var receivedFavoriteIds = await _context.Favorites
                .Where(f => f.ProfileId == myProfileId)
                .Select(f => f.UserId)
                .ToListAsync();

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

            // Взаимные лайки (Matches) — пересечение
            Matches = ReceivedLikes.Where(r => r.IsMatch).ToList();
        }

        public class LikeViewModel
        {
            public DateBoard.Models.Profile Profile { get; set; }
            public bool IsMatch { get; set; }
            public DateTime? LikeDate { get; set; }
        }
    }
}