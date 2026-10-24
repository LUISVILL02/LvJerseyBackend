using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Abstractions;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.Validations;
using Orders.Infrastructure.Services;
using Shared.Infrastructure.Extensions;

namespace Orders.Infrastructure.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IJerseyQueryService, JerseyQueryService>();
        services.AssemblyRegister(typeof(CreateOrderCommandHandler).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();
        return services;
    }
}
