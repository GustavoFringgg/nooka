using Microsoft.EntityFrameworkCore;
using Nooka.Api.Models;

namespace Nooka.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Word> Words { get; set; }
    public DbSet<Category> Categories { get; set; }
}