using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace URL_Shortener.Models.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ShortenedURL> ShortenedURLs { get; set; }=null!;
        public DbSet<ClickLog> ClickLogs { get; set; }=null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                    .HasMany(u => u.ShortenedURLs)
                    .WithOne(s => s.User)
                    .HasForeignKey(s => s.UserId);
        }

    }
}
