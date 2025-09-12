using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;
using Users.Application.Commands;

namespace Users.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddHandlersFromAssembly(typeof(UpdateInfoCommandHandler).Assembly);
        return services;
    }
}