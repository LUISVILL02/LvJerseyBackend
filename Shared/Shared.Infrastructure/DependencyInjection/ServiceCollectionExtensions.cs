using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Abstractions;
using Shared.Application.Settings;
using Shared.Infrastructure.Abstractions.Implementations;
using Shared.Infrastructure.BackgroundServices;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<IEmailSender, SendGridEmailSender>();

        // Registrar IHttpContextAccessor y el servicio de contexto de usuario
        services.AddScoped<IUserContextService, HttpContextUserContextService>();

        return services;
    }

    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar StorageSettings desde appsettings
        services.Configure<StorageSettings>(
            configuration.GetSection(StorageSettings.SectionName));

        // Registrar servicio de almacenamiento Storj (S3-compatible)
        services.AddSingleton<IBlobStorageService, StorjBlobService>();

        // Registrar cola en memoria para procesamiento de archivos
        services.AddSingleton<IFileUploadQueue, InMemoryFileUploadQueue>();

        // Registrar el BackgroundService para procesamiento de cola
        services.AddHostedService<FileUploadQueueProcessor>();

        return services;
    }
}