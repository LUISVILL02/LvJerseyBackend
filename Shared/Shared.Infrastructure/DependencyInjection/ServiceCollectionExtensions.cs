using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using Shared.Application.Abstractions;
using Shared.Application.Settings;
using Shared.Infrastructure.BackgroundServices;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // EF Core configuration
        });
        
        services.AddScoped<IUserContextService, HttpContextUserContextService>();
        services.AddScoped<IEmailSender, ResendEmailSender>();
        
        services.AddResend(options =>
        {
            options.ApiToken = configuration["Resend:ApiKey"] 
                ?? configuration["SendGrid:ApiKey"] 
                ?? Environment.GetEnvironmentVariable("RESEND_APITOKEN")
                ?? string.Empty;
        });
        
        services.AddSingleton<IFileUploadQueue, InMemoryFileUploadQueue>();
        services.AddHostedService<FileUploadQueueProcessor>();
        
        return services;
    }

    public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        services.AddSingleton<IBlobStorageService, StorjBlobService>();
        return services;
    }
    
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        services.AssemblyRegister(typeof(Shared.Application.Abstractions.ISender).Assembly);
        return services;
    }
}
