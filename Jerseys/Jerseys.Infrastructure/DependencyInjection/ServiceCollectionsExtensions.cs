using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Abstractions.Patches;
using Jerseys.Application.Commands.CreateJersey;
using Jerseys.Application.Queries.JerseysHome;
using Jerseys.Application.Validations;
using Jerseys.Infrastructure.Services;

namespace Jerseys.Infrastructure.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddJerseysModule(this IServiceCollection services)
    {
        // Registrar repositorios
        services.AddScoped<IJerseyRepository, JerseyRepository>();
        services.AddScoped<IPatchRepository, PatchRepository>();
        
        // Registrar handlers
        services.AssemblyRegister(typeof(HomeJerseysQueryHandler).Assembly);
        services.AssemblyRegister(typeof(CreateJerseyCommandHandler).Assembly);

        // Registrar validadores de FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateJerseyCommandValidator>();

        return services;
    }
}

