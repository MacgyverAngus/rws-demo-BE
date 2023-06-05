using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TranslationJob> TranslationJobs { get; set; }
        public DbSet<TranslatorModel> Translators { get; set; }
    }
}