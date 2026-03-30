using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DateBoard.Pages.Favorites
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser>
        _userManager;

        public AddModel(ApplicationDbContext db, UserManager<IdentityUser>
        userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult>
        OnPostAsync(int profileId)
        {
            var user = await _userManager.GetUserAsync(User);

            var already = _db.Favorites.Any(f => f.UserId == user!.Id && f.ProfileId == profileId);
            if (!already)
            {
                _db.Favorites.Add(new Favorite
                {
                    UserId = user!.Id,
                    ProfileId = profileId
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/Feed/Index");
        }
    }
}
