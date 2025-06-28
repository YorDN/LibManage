
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the User model. It represents a user in the library. Inherits from the IdentityUser
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            this.Id = Guid.NewGuid();
        }

        [Required]
        [Comment("The profile picture of the user")]
        public required string ProfilePicture { get; set; }

        [Required]
        [Comment("When was the user created")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Required]
        [Comment("When did the user last log in")]
        public DateTime LastLogin { get; set; }

        [Comment("Is the user's account active")]
        public bool IsActive { get; set; } = true;

        // A collection of the books taken by the user
        public ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

        // A collection of the reviews written by the user
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
