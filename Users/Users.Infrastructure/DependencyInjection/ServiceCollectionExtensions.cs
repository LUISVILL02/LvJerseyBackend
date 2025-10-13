using Authentication.Application.Abstractions.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;
using Users.Application;
using Users.Infrastructure.Services;

namespace Users.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AssemblyRegister(typeof(CommandHandlerAssemblyMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(CommandHandlerAssemblyMarker).Assembly);
        services.AddScoped<IUserAuthRepository, UserReadService>();
        return services;
    }
}