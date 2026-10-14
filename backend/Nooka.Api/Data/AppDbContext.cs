using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Nooka.Api.Models;

namespace Nooka.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
// TUser = AppUser:你的使用者實體長什麼樣子
// TRole = AppRole:你的角色實體長什麼樣子(繼承 IdentityRole<int>)
// TKey = int:上面兩個的主鍵型別用 int
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Word> Words { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<WordCategory> WordCategories { get; set; }
    public DbSet<WordProgress> WordProgresses { get; set; }
    public DbSet<DailyActivity> DailyActivities { get; set; }
    public DbSet<WordSm2Progress> WordSm2Progresses { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    // 微調欄位
    {
        base.OnModelCreating(modelBuilder);
        // Category
        modelBuilder.Entity<Category>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Category>()
            .Property(c => c.UpdatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Word
        modelBuilder.Entity<Word>()
            .HasIndex(w => w.Term)
            .IsUnique();

        // WordCategory
        modelBuilder.Entity<WordCategory>()
            .HasKey(wc => new { wc.WordId, wc.CategoryId });

        modelBuilder.Entity<WordCategory>()

            .HasOne<Word>()
            .WithMany()
            .HasForeignKey(wc => wc.WordId);

        modelBuilder.Entity<WordCategory>()
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(wc => wc.CategoryId);

        // WordProgress
        modelBuilder.Entity<WordProgress>()
            .HasKey(wp => new { wp.UserId, wp.WordId });

        modelBuilder.Entity<WordProgress>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(wp => wp.UserId);

        modelBuilder.Entity<WordProgress>()
            .HasOne<Word>()
            .WithMany()
            .HasForeignKey(wp => wp.WordId);

        modelBuilder.Entity<WordProgress>()
            .HasIndex(wp => new { wp.UserId, wp.NextReviewAt });

        modelBuilder.Entity<WordProgress>()
            .HasIndex(wp => new { wp.UserId, wp.IsArchived });

        modelBuilder.Entity<WordProgress>()
            .Property(wp => wp.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<WordProgress>()
            .Property(wp => wp.UpdatedAt)
            .HasDefaultValueSql("now()");

        // WordSm2Progress
        modelBuilder.Entity<WordSm2Progress>()
            .HasKey(wsp => new { wsp.UserId, wsp.WordId, wsp.Mode });

        modelBuilder.Entity<WordSm2Progress>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(wsp => wsp.UserId);

        modelBuilder.Entity<WordSm2Progress>()
            .HasOne<Word>()
            .WithMany()
            .HasForeignKey(wsp => wsp.WordId);

        // DailyActivity
        modelBuilder.Entity<DailyActivity>()
            .HasKey(da => new { da.UserId, da.ActivityDate, da.Mode });

        modelBuilder.Entity<DailyActivity>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(da => da.UserId);


    }
}
