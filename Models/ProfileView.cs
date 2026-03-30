using Microsoft.AspNetCore.Identity;

namespace DateBoard.Models;

public class ProfileView
{
    public int Id { get; set; }
    public string ViewerId { get; set; } = null!;  // ← required
    public int ProfileId { get; set; }
    public DateTime ViewedAt { get; set; }

    public IdentityUser? Viewer { get; set; }   // ← nullable
    public Profile? Profile { get; set; }       // ← nullable
}