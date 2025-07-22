using System.ComponentModel.DataAnnotations;

namespace LibManage.ViewModels.Rating
{
    public class ReviewInputModel
    {

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }
}
