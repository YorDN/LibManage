using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace LibManage.ViewModels.Books
{
    public class EditBookInputModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters.")]
        public string ISBN { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [StringLength(50, ErrorMessage = "Edition cannot exceed 50 characters.")]
        public string? Edition { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Language cannot exceed 100 characters.")]
        public string Language { get; set; }

        [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters.")]
        public string? Genre { get; set; }

        [Required]
        public string Type { get; set; }

        public TimeSpan? Duration { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public IFormFile? NewCover { get; set; }

        public IFormFile? NewBookFile { get; set; }

        [Required]
        public string ExistingCoverPath { get; set; }

        public string? ExistingFilePath { get; set; }

        [Required]
        public Guid AuthorId { get; set; }
        public List<AddBookAuthorViewModel> Authors { get; set; } = new List<AddBookAuthorViewModel>();

        [Required]
        public Guid PublisherId { get; set; }
        public List<AddBookPublisherViewModel> Publishers { get; set; } = new List<AddBookPublisherViewModel>();
    }
}
