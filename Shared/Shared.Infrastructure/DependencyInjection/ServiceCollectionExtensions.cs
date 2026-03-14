using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Abstractions.Implementations;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<IEmailSender, SendGridEmailSender>();
        //services.AddScoped<IUserContextService, UserContextService>();

        // Registrar IHttpContextAccessor y el servicio de contexto de usuario
        services.AddScoped<IUserContextService, HttpContextUserContextService>();

        return services;
    }
}