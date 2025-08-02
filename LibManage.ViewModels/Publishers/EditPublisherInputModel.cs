using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LibManage.ViewModels.Publishers
{
    public class EditPublisherInputModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [MaxLength(300)]
        [DisplayName("Country")]
        public string? Country { get; set; }

        [Url]
        [DisplayName("Website")]
        public string? Website { get; set; }

        public string ExistingLogoPath { get; set; } = null!;

        [Display(Name = "Change Logo")]
        public IFormFile? NewLogo { get; set; }
    }
}
