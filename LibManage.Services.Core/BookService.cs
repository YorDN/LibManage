
using LibManage.Data;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibManage.Services.Core
{
    public class BookService(ApplicationDbContext context,
        IRatingService ratingService) : IBookService
    {
        public Task<bool> CreateBookAsync(AddBookInputModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<AllBooksViewModel>?> GetAllBooksAsync()
        {
            IEnumerable<AllBooksViewModel>? allBooksViewModel = await context.Books
                .Include(d => d.Author)
                .AsNoTracking()
                .Select(b => new AllBooksViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author.FullName,
                    BookType = b.Type.ToString(),
                    Cover = b.Cover
                })
                .ToListAsync();
            if (allBooksViewModel is  null ) 
                return allBooksViewModel;

            for ( int i = 0; i < allBooksViewModel.Count(); i++ )
            {
                double? rating = await ratingService.GetRatingForABookByIdAsync(allBooksViewModel.ToList()[i].Id);
                if ( rating != null)
                {
                    allBooksViewModel.ToList()[i].Rating = (int)rating;
                }
            }

            return allBooksViewModel;
        }
    }
}
