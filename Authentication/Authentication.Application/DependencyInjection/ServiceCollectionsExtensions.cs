using Authentication.Application.Commands;
using Authentication.Application.Commands.Register;
using Authentication.Application.Validations;
using Authentication.Infrastructure.Abstractions;
using Authentication.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;

namespace Authentication.Application.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtUtil, JwtUtil>();
        services.AddHandlersFromAssembly(typeof(RegisterCommandHandler).Assembly);
        services.AddValidatorsFromAssembly(typeof(RegisterCommandValidator).Assembly);
        return services;
    }
}