using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LibManage.Data.Models.Library
{
    public class UserEpubProgress
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public Guid BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        [Required]
        [Comment("User's chapter progress")]
        public int LastChapterIndex { get; set; }

        [Required]
        [Comment("When was the progress last updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
