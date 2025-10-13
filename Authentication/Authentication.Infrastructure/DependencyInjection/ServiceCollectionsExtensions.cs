using Authentication.Application.Abstractions;
using Authentication.Application.Commands.Register;
using Authentication.Application.Validations;
using Authentication.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;

namespace Authentication.Infrastructure.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services)
    {
        services.AddScoped<IJwtUtil, JwtUtil>();
        services.AddScoped<IExternalAuthValidator, ExternalAuthValidator>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AssemblyRegister(typeof(RegisterCommandHandler).Assembly);
        services.AddValidatorsFromAssembly(typeof(RegisterCommandValidator).Assembly);
        return services;
    }
}