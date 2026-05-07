using DateBoard.Data;
using DateBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DateBoard.Pages.Profile
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public Models.Profile ProfileModel { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            ProfileModel = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (ProfileModel == null)
            {
                ProfileModel = new Models.Profile { UserId = user.Id };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? Photo)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (Photo != null && Photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{user.Id}_{Path.GetFileName(Photo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await Photo.CopyToAsync(stream);
                ProfileModel.PhotoPath = $"/uploads/{fileName}";
            }
            else if (existing != null)
            {
                ProfileModel.PhotoPath = existing.PhotoPath;
            }

            if (existing == null)
            {
                ProfileModel.UserId = user.Id;
                ProfileModel.CreatedAt = DateTime.UtcNow;
                _db.Profiles.Add(ProfileModel);
            }
            else
            {
                existing.Name = ProfileModel.Name;
                existing.City = ProfileModel.City;
                existing.Age = ProfileModel.Age;
                existing.About = ProfileModel.About;
                existing.Goal = ProfileModel.Goal;
                existing.HasChildren = ProfileModel.HasChildren;
                existing.Hobbies = ProfileModel.Hobbies;
                existing.MeetingNotes = ProfileModel.MeetingNotes;
                existing.PhotoPath = ProfileModel.PhotoPath;
            }

            await _db.SaveChangesAsync();
            return RedirectToPage("/Profile/Edit");
        }
    }
}