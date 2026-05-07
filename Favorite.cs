using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DateBoard.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public IdentityUser? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ProfileId { get; set; }
        [ForeignKey("ProfileId")]
        public Profile? Profile { get; set; }
    }
}