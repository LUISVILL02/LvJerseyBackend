using Microsoft.Extensions.DependencyInjection;
using Reviews.Application.Abstractions;
using Reviews.Infrastructure.Services;

namespace Reviews.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReviewsModule(this IServiceCollection services)
    {
        services.AddScoped<IReviewRepository, ReviewRepository>();
        return services;
    }
}
