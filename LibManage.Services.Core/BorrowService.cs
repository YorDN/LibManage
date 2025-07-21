using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Borrows;

using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class BorrowService(ApplicationDbContext context) : IBorrowService
    {
        public async Task<IEnumerable<BorrowedBookViewModel>?> GetUsersBorrowedBooksAsync(Guid userId)
        {
            User? user = await context.Users
                .FindAsync(userId);

            if (user == null) 
                return null;

            IEnumerable<Borrow> borrows = await context.Borrows
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            DateTime now = DateTime.UtcNow;

            foreach (var borrow in borrows.Where(b => !b.Returned && b.DateDue < now))
            {
                borrow.Returned = true;
                borrow.DateReturned = now;
            }

            if (context.ChangeTracker.HasChanges())
                await context.SaveChangesAsync();

            return borrows
                .Where(b => !b.Returned)
                .Select(b => new BorrowedBookViewModel
                {
                    BorrowId = b.Id,
                    BookId = b.Book.Id,
                    Title = b.Book.Title,
                    Cover = b.Book.Cover,
                    DateTaken = b.DateTaken,
                    DateDue = b.DateDue,
                    BookType = b.Book.Type.ToString()
                })
                .ToList();

        }

        public async Task<bool> HasActiveBorrowsAsync(Guid? userId, Guid bookId)
        {
            if (userId == null) 
                return false;
            return await context.Borrows
            .AnyAsync(b => b.UserId == userId && b.BookId == bookId && !b.Returned);

        }
        public async Task<bool> RentBookAsync(Guid userId, Guid bookId)
        {
            Book? book = await context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId);
            User? user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || book == null) 
                return false;
            bool alreadyBorrowed = false;
            
            if (book.Type != Book.BookType.Physical)
            {
                alreadyBorrowed = await context.Borrows
                .AnyAsync(br => br.BookId == bookId && br.UserId == userId && !br.Returned);
            }
            else
            {
                alreadyBorrowed = await context.Borrows
                .AnyAsync(br => br.BookId == bookId && !br.Returned);
            }

            if (alreadyBorrowed)
                return false;


            Borrow borrow = new Borrow() 
            { 
                UserId = userId,
                BookId = bookId,
                DateTaken = DateTime.UtcNow,
                DateDue = DateTime.UtcNow.AddDays(14),
                Returned = false
            };

            context.Borrows.Add(borrow);
            user.Borrows.Add(borrow);
            book.Borrows.Add(borrow);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReturnBookAsync(Guid userId, Guid borrowId)
        {
            User? user = await context.Users
                .FindAsync(userId);
            if (user == null)
                return false;

            Borrow? borrow = await context.Borrows
                .FirstOrDefaultAsync(br => br.Id == borrowId);

            if (borrow == null) 
                return false;

            borrow.DateReturned = DateTime.UtcNow;
            borrow.Returned = true;
            context.Borrows.Update(borrow);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
