using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Abstractions;
using Payments.Infrastructure.Services;
using Shared.Infrastructure.Extensions;

namespace Payments.Infrastructure.DependencyInjection;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        services.AssemblyRegister(typeof(PaymentService).Assembly);
        return services;
    }
}
