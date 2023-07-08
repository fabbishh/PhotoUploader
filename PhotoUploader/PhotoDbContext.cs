using Microsoft.EntityFrameworkCore;
using PhotoUploader.Entities;

namespace PhotoUploader
{
    public class PhotoDbContext : DbContext
    {
        public PhotoDbContext(DbContextOptions<PhotoDbContext> options) : base(options) { }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация моделей и таблиц базы данных
            modelBuilder.Entity<Photo>().ToTable("Photos")
                .HasKey(p => p.Id);
            modelBuilder.Entity<User>().ToTable("Users")
                .HasKey(u => u.Id);
        }
    }
}
