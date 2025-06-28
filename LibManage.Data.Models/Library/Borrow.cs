using System.ComponentModel.DataAnnotations;

namespace LibManage.Data.Models.Library
{
    /// <summary>
    ///     This is the Borrow model. It represents the book-user relashionship.
    /// </summary>
    public class Borrow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MyProperty { get; set; }
    }
}