using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Queries.JerseysHome;
using Jerseys.Infrastructure.Services;

namespace Jerseys.Infrastructure.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddJerseysModule(this IServiceCollection services)
    {
        services.AddScoped<IJerseyRepository, GetJerseysByCategory>();
        services.AssemblyRegister(typeof(HomeJerseysQueryHandler).Assembly);

        // Si hay validadores con FluentValidation, usar AddValidatorsFromAssembly
        // services.AddValidatorsFromAssembly(typeof(SomeValidator).Assembly);

        return services;
    }
}

