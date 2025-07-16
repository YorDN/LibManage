using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibManage.ViewModels.Authors
{
    public class AddAuthorInputModel
    {
        [Required]
        [MaxLength(500)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Photo")]
        public IFormFile PhotoFile { get; set; }

        [Display(Name = "Biography")]
        public string? Biography { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; } = null;

        [Display(Name = "Date of Death")]
        [DataType(DataType.Date)]
        public DateTime? DateOfDeath { get; set; } = null;
    }
}
