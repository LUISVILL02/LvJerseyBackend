using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Abstractions;
using Shared.Infrastructure.Abstractions.Implementations;

namespace Shared.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        return services;
    }
}