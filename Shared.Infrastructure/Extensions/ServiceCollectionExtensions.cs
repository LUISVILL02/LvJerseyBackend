using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Abstractions;

namespace Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(t => new
            {
                Type = t,
                Interfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                                (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                                 i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            });

        foreach (var ht in handlerTypes)
        {
            foreach (var @interface in ht.Interfaces)
            {
                services.AddScoped(@interface, ht.Type);
            }
        }

        return services;
    }
}