using Microsoft.EntityFrameworkCore;

namespace YapYapAPI.Data
{
    public class YapYapDbContext : DbContext
    {
        public YapYapDbContext(DbContextOptions<YapYapDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.BIO).HasMaxLength(500);
                entity.Property(e => e.status_id).IsRequired();
                entity.Property(e => e.created_at).IsRequired();
            });
        }
    }
}