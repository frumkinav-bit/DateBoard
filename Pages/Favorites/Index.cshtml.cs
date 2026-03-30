using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Pages.Favorites
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

        public List<Favorite> Favorites { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Favorites = await _db.Favorites
                .Where(f => f.UserId == user!.Id)
                .Include(f => f.Profile)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int favoriteId)
        {
            var fav = await _db.Favorites.FindAsync(favoriteId);
            if (fav != null)
            {
                _db.Favorites.Remove(fav);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}