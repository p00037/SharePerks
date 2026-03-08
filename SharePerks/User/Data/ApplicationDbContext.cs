using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace User.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RewardItem>();
            modelBuilder.Entity<Shareholder>();
            modelBuilder.Entity<RewardOrder>();
            modelBuilder.Entity<RewardOrderItem>();
        }

        public DbSet<RewardItem> RewardItems => Set<RewardItem>();
    }
}
