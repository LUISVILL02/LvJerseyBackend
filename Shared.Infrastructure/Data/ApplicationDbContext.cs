using Microsoft.EntityFrameworkCore;

namespace Shared.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Importar configuraciones de cada módulo
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(Usuarios.Infrastructure.AssemblyMarker).Assembly);
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(Pedidos.Infrastructure.AssemblyMarker).Assembly);
    }
};