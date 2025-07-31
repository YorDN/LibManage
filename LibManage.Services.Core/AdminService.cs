using LibManage.Data;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;
using static LibManage.Data.Models.Library.Book;

namespace LibManage.Services.Core
{
    public class AdminService(ApplicationDbContext context) : IAdminService
    {
        public async Task<AdminDashboardViewModel> GetAdminDashboardDetailsAsync()
        {
            AdminDashboardViewModel model = new AdminDashboardViewModel();

            model.PhysicalBooks = await context.Books
                .AsNoTracking()
                .Where(b => b.Type == BookType.Physical)
                .CountAsync();
            model.AudioBooks = await context.Books
                .AsNoTracking()
                .Where(b => b.Type == BookType.Audio)
                .CountAsync();
            model.DigitalBooks = await context.Books
                .AsNoTracking()
                .Where(b => b.Type == BookType.Digital)
                .CountAsync();
            model.TotalUsers = await context.Users
                .AsNoTracking()
                .CountAsync();
            model.TotalPublishers = await context.Publishers
                .AsNoTracking()
                .CountAsync();
            model.TotalBooks = await context.Books
                .AsNoTracking()
                .CountAsync();
            model.TotalAuthors = await context.Authors
                .AsNoTracking()
                .CountAsync();
            model.ActiveUsersLast30Days = await context.Users
                .AsNoTracking()
                .CountAsync(u => u.LastLogin >= DateTime.UtcNow.AddDays(-30));
            model.MostActiveUser = await context.Users
                .Include(u => u.Borrows)
                .OrderByDescending(u => u.Borrows.Count)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
            int repeatBorrowers = await context.Users
                .Where(u => u.Borrows.Count() > 1)
                .CountAsync();
            int totalBorrowers = await context.Users
                .Where(u => u.Borrows.Any())
                .CountAsync();
            model.RepeatBorrowersPercent = totalBorrowers == 0
                ? 0
                : (double) repeatBorrowers / totalBorrowers * 100;
            model.RecentBooks = await context.Books
                .OrderByDescending(b => b.UploadDate)
                .Take(5)
                .Include(b => b.Author)
                .Select(b => new RecentBookViewModel
                {
                    Title = b.Title,
                    AuthorName = b.Author.FullName,
                    DateAdded = b.UploadDate
                })
                .ToListAsync();
            model.MostBorrowedBook = await context.Books
                .Include(b => b.Borrows)
                .OrderByDescending (b => b.Borrows.Count())
                .Select(b => b.Title)
                .FirstOrDefaultAsync();
            var reviewData = await context.Reviews
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = $"{g.Key.Month:D2}/{g.Key.Year}",
                    Count = g.Count()
                })
                .ToListAsync();
            model.ReviewMonths = reviewData
                .Select(d => d.Month)
                .ToList();
            model.ReviewCounts = reviewData
                .Select(d => d.Count)
                .ToList();

            return model;
        }
    }
}
