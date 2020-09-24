using Celebrities.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Celebrities.Database
{
    public class CelebritiesDbContext : DbContext
    {
        public DbSet<Celebrity> Celebrities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public CelebritiesDbContext(DbContextOptions<CelebritiesDbContext> dbContextOptions) : base(dbContextOptions)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(new Role {Id = 1, Name = "Admin"});
            modelBuilder.Entity<User>().HasData(new User {Id = 1, Name = "Admin", Login = "Admin", Password = "Admin", RoleId = 1});
        }
    }
}