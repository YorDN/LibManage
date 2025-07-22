using LibManage.Data.Models.Library;
using LibManage.ViewModels.Rating;

namespace LibManage.ViewModels.Books
{
    public class BookDetailsViewModel
    {
        public Book Book { get; set; }
        public bool IsTaken { get; set; }
        public bool IsTakenByUser { get; set; }
        public double AverageRating { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
        public int TotalReviewCount { get; set; }
        public int CurrentReviewPage { get; set; }
        public ReviewInputModel NewReview { get; set; } = new ReviewInputModel();
        public bool CanReview { get; set; }
    }
}
