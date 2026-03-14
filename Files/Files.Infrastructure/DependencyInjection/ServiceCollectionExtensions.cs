using Files.Application.Abstractions;
using Files.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Files.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFilesModule(this IServiceCollection services)
    {
        // Registrar repositorio de archivos
        services.AddScoped<IFileRepository, FileRepository>();

        return services;
    }
}

