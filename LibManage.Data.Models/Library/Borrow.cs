using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the Borrow model. It represents the book-user borrowing relashionship.
    /// </summary>
    public class Borrow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        [Required]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public Guid BookId { get; set; }
        [Required]
        [ForeignKey(nameof(BookId))]
        public Book Book { get; set; } = null!;

        [Comment("When was the book taken by the user")]
        public DateTime DateTaken { get; set; }

        [Comment("The due date by which the book should be returned")]
        public DateTime DateDue { get; set; }

        [Comment("Is the book returned")]
        public bool Returned { get; set; } = false;

        [Comment("When was the book returned")]
        public DateTime? DateReturned { get; set; }
    }
}