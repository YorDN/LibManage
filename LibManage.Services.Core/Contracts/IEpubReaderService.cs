using LibManage.ViewModels.Reader;

namespace LibManage.Services.Core.Contracts
{
    public interface IEpubReaderService
    {
        public Task<EpubReadViewModel?> LoadChapterAsync(Guid bookId, Guid userId, int? chapterIndex);

    }
}
