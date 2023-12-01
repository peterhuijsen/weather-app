using App.Models;
using App.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.Database.Contexts;

public class UserContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    private readonly string _connectionString;
    
    public UserContext(IOptions<Configuration> configuration)
        => _connectionString = configuration.Value.Database.Connection;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(_connectionString);
}