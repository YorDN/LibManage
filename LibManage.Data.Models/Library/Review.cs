
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the review model. It represents a review, written by a user.
    /// </summary>
    public class Review
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [Required]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public Guid BookId { get; set; }
        [Required]
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        [Comment("The rating of the review")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        [Comment("The comment of the review")]
        public string? Comment { get; set; }

        [Comment("When was the review created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}