using Microsoft.AspNetCore.Identity;

namespace DateBoard.Models;

public class Like
{
    

    public int Id { get; set; }
    public string FromUserId { get; set; } = null!;  // ← required (C# 11)
    public int ToProfileId { get; set; }
    public DateTime CreatedAt { get; set; }

    public IdentityUser? FromUser { get; set; }  // ← nullable
    public Profile? ToProfile { get; set; }      // ← nullable
}