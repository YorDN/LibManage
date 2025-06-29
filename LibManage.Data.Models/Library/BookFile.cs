
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the BookFile model. It stores the file metadata of 
    ///     the audio/digital books in the library
    /// </summary>
    public class BookFile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookId { get; set; }
        [Required]
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        [Required]
        [Comment("Where is the book file located on the server (digital/audio books)")]
        public required string FilePath { get; set; }

        [Required]
        [Comment("The size of the book file")]
        public long FileSize { get; set; }

        [Comment("MIME type of the book file")]
        public string? MimeType { get; set; }

        [Comment("When was the book file uploaded")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public enum FormatType
        {
            Epub,
            Pdf,
            Mp3,
            Other
        }

        [Required]
        [EnumDataType(typeof(FormatType))]
        public FormatType Format { get; set; }
    }


}

