using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DateBoard.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }

        [Required, Display(Name = "Имя")]
        public string Name { get; set; } = string.Empty;

        [Required, Display(Name = "Город")]
        public string City { get; set; } = string.Empty;

        [Required, Display(Name = "Возраст")]
        [Range(18, 100)]
        public int Age { get; set; }

        [Display(Name = "О себе")]
        public string? About { get; set; }

        [Display(Name = "Цель знакомства")]
        public string? Goal { get; set; }

        [Display(Name = "Наличие детей")]
        public bool HasChildren { get; set; }

        [Display(Name = "Увлечения")]
        public string? Hobbies { get; set; }

        [Display(Name = "Фото")]
        public string? PhotoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Заметки о встречах")]
        public string? MeetingNotes { get; set; }
    }
}