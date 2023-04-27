using Microsoft.EntityFrameworkCore;

namespace ASP_NET.Data;

public class DataContext : DbContext
{
    public DbSet<Entity.User?> Users { get; set; }
    public DbSet<Entity.EmailConfirmToken?> EmailConfirmTokens { get; set; }

    public DataContext(DbContextOptions options) : base(options)
    {
    }
}