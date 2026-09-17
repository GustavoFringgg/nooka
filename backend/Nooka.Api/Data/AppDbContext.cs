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
    public DbSet<WordCategory> WordCategories{get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    // 微調欄位
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Category>()
            .Property(c => c.UpdatedAt)
            .HasDefaultValueSql("now()");
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
        modelBuilder.Entity<Word>()
            .HasIndex(w => w.Term)
            .IsUnique(); 
    }
}
