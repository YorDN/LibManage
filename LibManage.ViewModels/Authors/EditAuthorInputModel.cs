using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LibManage.ViewModels.Authors
{
    public class EditAuthorInputModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Biography")]
        [MaxLength(2000)]
        public string? Biography { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Date of Death")]
        [DataType(DataType.Date)]
        public DateTime? DateOfDeath { get; set; }

        public string ExistingPhotoPath { get; set; } = null!;

        [Display(Name = "Change Photo")]
        public IFormFile? NewPhoto { get; set; }
    }
}
