using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class TranslationJobRepository : ITranslationJobRepository, IDisposable
    {
        private AppDbContext _dbContext;
        private bool disposed = false;
        public TranslationJobRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public TranslationJob? GetTranslationJob(int id)
        {
            return _dbContext.TranslationJobs.FirstOrDefault(x => x.Id == id);
        }

        public IReadOnlyCollection<TranslationJob> GetTranslationJobs()
        {
            return _dbContext.TranslationJobs.ToArray();
        }

        public void SaveTranslationJob(TranslationJob job)
        {
            _dbContext.TranslationJobs.Add(job);
            //return _dbContext.SaveChanges() > 0;
        }
        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}