using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LibManage.ViewModels.Books
{
    public class AddBookInputModel
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [MaxLength(100)]
        public string? Edition { get; set; }


        [Required]
        [MaxLength(100)]
        public string Language { get; set; }

        [MaxLength(100)]
        public string? Genre { get; set; }

        [Required]
        public string Type { get; set; }

        public TimeSpan? Duration { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public IFormFile? CoverFile { get; set; }

        public IFormFile? BookFile { get; set; }

        [Required]
        public Guid AuthorId { get; set; }
        public List<AddBookAuthorViewModel> Authors { get; set; } = new List<AddBookAuthorViewModel>();

        [Required]
        public Guid PublisherId { get; set; }
        public List<AddBookPublisherViewModel> Publishers { get; set; } = new List<AddBookPublisherViewModel>();
    }

}
