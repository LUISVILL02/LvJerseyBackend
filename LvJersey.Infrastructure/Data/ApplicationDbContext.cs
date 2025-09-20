using Microsoft.EntityFrameworkCore;

namespace LvJersey.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Importar configuraciones de cada módulo
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Users.Infrastructure.AssemblyMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }
};