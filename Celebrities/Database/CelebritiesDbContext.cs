using Celebrities.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Celebrities.Database
{
    public class CelebritiesDbContext : DbContext
    {
        public DbSet<Celebrity> Celebrities { get; set; }

        public CelebritiesDbContext(DbContextOptions<CelebritiesDbContext> dbContextOptions) : base(dbContextOptions)
        {
            Database.EnsureCreated();
        }
    }
}