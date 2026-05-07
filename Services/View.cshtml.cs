using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Pages.Profile;

public class ViewModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ViewModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Models.Profile? Profile { get; set; }
    public bool IsOnline { get; set; }
    public int LastSeenMinutes { get; set; }
    public int CompatibilityScore { get; set; }
    public int ViewsCount { get; set; }
    public int LikesCount { get; set; }
    public int FavoritesCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        // Проверка на null
        if (string.IsNullOrEmpty(id)) return NotFound();
 

        Profile = await _db.Profiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == id || p.Id.ToString() == id);

        if (Profile == null)
        {
            return NotFound();
        }

        var random = new Random(Profile.Id);
        IsOnline = random.Next(3) == 0;
        LastSeenMinutes = random.Next(1, 120);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null && Profile.UserId != currentUser.Id)
        {
            CompatibilityScore = CalculateCompatibility(currentUser.Id, Profile);
            await RecordView(currentUser.Id, Profile.Id);
        }

        if (currentUser != null && Profile.UserId == currentUser.Id)
        {
            ViewsCount = await _db.ProfileViews.CountAsync(v => v.ProfileId == Profile.Id);
            LikesCount = await _db.Likes.CountAsync(l => l.ToProfileId == Profile.Id);
            FavoritesCount = await _db.Favorites.CountAsync(f => f.ProfileId == Profile.Id);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostToggleFavoriteAsync(int profileId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToPage("/Account/Login");

        var existing = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.ProfileId == profileId);

        if (existing != null)
        {
            _db.Favorites.Remove(existing);
        }
        else
        {
            _db.Favorites.Add(new Favorite
            {
                UserId = currentUser.Id,
                ProfileId = profileId,
                AddedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        return RedirectToPage(new { id = profileId });
    }

    public async Task<IActionResult> OnPostLikeAsync(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToPage("/Account/Login");

        var existing = await _db.Likes
            .FirstOrDefaultAsync(l => l.FromUserId == currentUser.Id && l.ToProfileId == id);

        if (existing == null)
        {
            _db.Likes.Add(new Like
            {
                FromUserId = currentUser.Id,
                ToProfileId = id,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    // ИСПРАВЛЕНО: убран мусор в конце метода
    public async Task<IActionResult> OnPostSaveNotesAsync(int profileId, string notes)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToPage("/Account/Login");

        var profile = await _db.Profiles.FindAsync(profileId);
        if (profile != null && profile.UserId != currentUser.Id)
        {
            profile.MeetingNotes = notes?.Substring(0, Math.Min(notes?.Length ?? 0, 500));
            await _db.SaveChangesAsync();
        }

        return RedirectToPage(new { id = profileId });
    }

    private int CalculateCompatibility(string userId, Models.Profile targetProfile)
    {
        var random = new Random(userId.GetHashCode() + targetProfile.Id);
        return random.Next(65, 99);
    }

    private async Task RecordView(string viewerId, int profileId)
    {
        var recent = await _db.ProfileViews
            .FirstOrDefaultAsync(v => v.ViewerId == viewerId &&
                                     v.ProfileId == profileId &&
                                     v.ViewedAt > DateTime.Now.AddHours(1));

        if (recent == null)
        {
            _db.ProfileViews.Add(new ProfileView
            {
                ViewerId = viewerId,
                ProfileId = profileId,
                ViewedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
    }
}