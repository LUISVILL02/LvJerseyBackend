using Microsoft.Extensions.DependencyInjection;

namespace Files.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFilesModule(this IServiceCollection services)
    {
        // Register services, repositories, etc here if/when they are created.
        // For now, it's just ensuring the assembly is reachable if we use scanning, 
        // but the DbContext scanner uses AppDomain.GetAssemblies(). 
        // We need to reference this project in the API so that the assembly is loaded.
        
        return services;
    }
}

