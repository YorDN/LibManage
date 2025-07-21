using LibManage.Data;
using LibManage.Data.Models.Library;
using LibManage.Services.Core.Contracts;
using LibManage.ViewModels.Reader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using VersOne.Epub;

namespace LibManage.Services.Core
{
    public class EpubReaderService(ApplicationDbContext context,
        IWebHostEnvironment env) : IEpubReaderService
    {
        public async Task<EpubReadViewModel?> LoadChapterAsync(Guid bookId, Guid userId, int? chapterIndex)
        {
            Book? book = await context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null || book.Type != Book.BookType.Digital || string.IsNullOrWhiteSpace(book.BookFilePath))
                return null;

            var borrow = await context.Borrows
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId && !b.Returned);

            if (borrow == null)
                return null;

            if (borrow.DateDue < DateTime.UtcNow)
            {
                borrow.Returned = true;
                borrow.DateReturned = DateTime.UtcNow;
                context.Borrows.Update(borrow);
                await context.SaveChangesAsync();
                return null;
            }

            string relativePath = book.BookFilePath.TrimStart('/');
            string filePath = Path.Combine(env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(filePath))
                return null;

            EpubBook epubBook = await EpubReader.ReadBookAsync(filePath);
            var chapters = epubBook.ReadingOrder;
            if (chapters.Count == 0)
                return null;

            UserEpubProgress? progress = await context.UserEpubProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.BookId == bookId);

            int index = chapterIndex ?? progress?.LastChapterIndex ?? 0;
            index = Math.Clamp(index, 0, chapters.Count - 1);

            var chapter = chapters[index];
            string html = chapter.Content ?? "<p>No content available.</p>";

            string chapterTitle = FindTitleFromToc(epubBook.Navigation, chapter.FilePath) ?? $"Chapter {index + 1}";

            if (progress == null)
            {
                progress = new UserEpubProgress
                {
                    UserId = userId,
                    BookId = bookId,
                    LastChapterIndex = index,
                    LastUpdated = DateTime.UtcNow
                };
                context.UserEpubProgresses.Add(progress);
            }
            else
            {
                progress.LastChapterIndex = index;
                progress.LastUpdated = DateTime.UtcNow;
                context.UserEpubProgresses.Update(progress);
            }

            await context.SaveChangesAsync();

            return new EpubReadViewModel
            {
                BookId = bookId,
                Title = epubBook.Title,
                ChapterIndex = index,
                ChapterCount = chapters.Count,
                ChapterTitle = chapterTitle,
                ChapterHtmlContent = html,
                TableOfContents = chapters.Select((c, i) =>
                    FindTitleFromToc(epubBook.Navigation, c.FilePath) ?? $"Chapter {i + 1}"
                ).ToList()
            };
        }

        private string? FindTitleFromToc(IEnumerable<EpubNavigationItem> tocItems, string chapterFilePath)
        {
            foreach (var item in tocItems)
            {
                if (item.Link?.ContentFilePath == chapterFilePath)
                    return item.Title;

                if (item.NestedItems?.Count > 0)
                {
                    var nested = FindTitleFromToc(item.NestedItems, chapterFilePath);
                    if (nested != null)
                        return nested;
                }
            }
            return null;
        }
    }
}
