using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DateBoard.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}