using DomainObjects;

namespace Repositories
{
    public interface ITranslationJobRepository : IDisposable
    {
        TranslationJob? GetTranslationJob(int id);
        IReadOnlyCollection<TranslationJob> GetTranslationJobs();
        void SaveTranslationJob(TranslationJob job);
        int Save();
    }
}