
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the publisher model. It represents a publisher in the library.
    /// </summary>
    public class Publisher
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        public required string Name { get; set; }

        [Required]
        [Comment("The logo of the publisher")]
        public required string LogoUrl { get; set; }

        [Comment("The description of the publisher")]
        public string? Description { get; set; }

        [MaxLength(300)]
        [Comment("The country of the publisher")]
        public string? Country { get; set; }

        [Url]
        [Comment("The website of the publisher")]
        public string? Website { get; set; }

        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}