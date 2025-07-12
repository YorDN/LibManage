using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibManage.ViewModels
{
    public class AddPublisherInputModel
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        [DisplayName("Logo / Profile picture")]
        public required string LogoUrl { get; set; }

        [MaxLength(1000)]
        [DisplayName("Description")]
        public string? Description { get; set; }

        [MaxLength(300)]
        [DisplayName("Country")]
        public string? Country { get; set; }

        [Url]
        [DisplayName("Website")]
        public string? Website { get; set; }
    }
}
