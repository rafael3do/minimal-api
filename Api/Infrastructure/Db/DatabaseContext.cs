using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entity;

namespace MinimalApi.Infrastructure.Db;

public class DatabaseContext : DbContext
{
    private readonly IConfiguration _configurationAppSettings;

    public DatabaseContext(IConfiguration _configurationAppSettings)
    {
        this._configurationAppSettings = _configurationAppSettings;
    }
    
    public DbSet<Administrator> Administrators { get; set; } = default!;

    public DbSet<Vehicle> Vehicles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>().HasData(
            new Administrator {
                Id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Adm",
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configurationAppSettings.GetConnectionString("SqlServer")?.ToString();
            if(!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseSqlServer(
                    stringConexao //,
                    // ServerVersion.AutoDetect(stringConexao)
                );
            }
        }
    }
}