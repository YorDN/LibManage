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
        // This column contains when and where the author was born, what are some of their important works etc.
        public string? Biography { get; set; }

        public ICollection<Book> WrittenBooks { get; set; } = new List<Book>();

    }
}