using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibManage.Data.Models.Library
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Comment("The title of the book")]
        public required string Title { get; set; }

        [Required]
        [MaxLength(20)]
        [Comment("The ISBN of the book")]
        public required string ISBN { get; set; }

        [Comment("The release date of the book")]
        public DateTime? ReleaseDate { get; set; }

        [MaxLength(50)]
        [Comment("The edition of the book")]
        // The edition is a string because it can be a number or a word (e.g. "First") and
        // also it can be special editions like "Special Edition" or "Anniversary Edition"
        public string? Edition { get; set; }

        [Required]
        [MaxLength(100)]
        [Comment("The language of the book")]
        public required string Language { get; set; }

        [MaxLength(100)]
        [Comment("The genre of the book")]
        public string? Genre { get; set; }

        // Limitating the types of books to only three options: Physical, Digital and Audio
        public enum BookType
        {
            Physical,
            Digital,
            Audio
        }
        [Required]
        [Comment("The type of the book")]
        // The Type is required, because every book has a type
        // Default value is Physical, as most books are physical
        public BookType Type { get; set; } = BookType.Physical;

        [Comment("The duration of the book")]
        public TimeSpan? Duration { get; set; }

        [MaxLength(1000)]
        [Comment("A short description of the book")]
        public string? Description { get; set; }

        [Required]
        [Comment("The cover of the book")]
        // The cover is required because every book must have a cover
        // If a book does not have a cover, a default cover will be used
        public required string Cover { get; set; }

        [Required]
        public string AuthorId { get; set; } = null!;
        [Required]
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; } = null!;

        [Required]
        public string PublisherId { get; set; } = null!;
        [Required]
        [ForeignKey(nameof(PublisherId))]
        public Publisher Publisher { get; set; } = null!;

        public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
