using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DateBoard.Models
{
    public class PrivateMessage
    {
        [Key]
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        [ForeignKey("SenderId")]
        public IdentityUser? Sender { get; set; }
        [ForeignKey("ReceiverId")]
        public IdentityUser? Receiver { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}