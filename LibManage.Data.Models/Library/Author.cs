using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the Author model. It represents an author in the library.
    /// </summary>
    public class Author
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        [Comment("The name of the author")]
        public required string FullName { get; set; }

        [Required]
        [Comment("The photo of the author")]
        public required string Photo { get; set; }

        [Comment("The biography of the author")]
        // This column contains some useful info about the author.
        public string? Biography { get; set; }

        [Comment("Date of birth of the author (optional)")]
        public DateTime? DateOfBirth { get; set; }

        [Comment("Date of death of the author (optional)")]
        public DateTime? DateOfDeath { get; set; }

        public ICollection<Book> WrittenBooks { get; set; } = new List<Book>();

    }
}