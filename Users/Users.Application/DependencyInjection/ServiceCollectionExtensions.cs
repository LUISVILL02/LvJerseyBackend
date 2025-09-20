using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;

namespace Users.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddHandlersFromAssembly(typeof(CommandHandlerAssemblyMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(CommandHandlerAssemblyMarker).Assembly);
        return services;
    }
}