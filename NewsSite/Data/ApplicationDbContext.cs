using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsSite.Models.Entities;

namespace NewsSite.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<ArticleLike> ArticleLikes { get; set; }
        public DbSet<UnsubscribeLog> UnsubscribeLogs { get; set; }

        public DbSet<NewsletterPreference> NewsletterPreferences { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<NewsletterPreference>()
                .HasIndex(n => n.UserId)
                .IsUnique();

            builder.Entity<UnsubscribeLog>()
                   .HasIndex(l => l.UserId);
            builder.Entity<UnsubscribeLog>()
                   .HasIndex(l => l.UnsubscribedAt);
        }
    }
}