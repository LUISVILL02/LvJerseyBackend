using Files.Application.Abstractions;
using Files.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions;
using Shared.Application.Dtos;
using Shared.Application.Settings;

namespace Shared.Infrastructure.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que procesa los mensajes de la cola de subida de archivos.
/// Lee mensajes de la cola en memoria, sube los archivos a Storj y actualiza la base de datos.
/// </summary>
public sealed class FileUploadQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileUploadQueue _fileUploadQueue;
    private readonly ILogger<FileUploadQueueProcessor> _logger;
    private readonly StorageSettings _settings;

    public FileUploadQueueProcessor(
        IServiceProvider serviceProvider,
        IFileUploadQueue fileUploadQueue,
        IOptions<StorageSettings> settings,
        ILogger<FileUploadQueueProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _fileUploadQueue = fileUploadQueue;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FileUploadQueueProcessor iniciado.");

        try
        {
            // Consumir mensajes de la cola de forma asíncrona
            await foreach (var message in _fileUploadQueue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error al procesar mensaje para archivo {FileId}",
                        message.IdFile);

                    // Marcar el archivo como fallido
                    await MarkFileAsFailedAsync(
                        message.IdFile,
                        $"Error al procesar: {ex.Message}",
                        stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("FileUploadQueueProcessor detenido por cancelación.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fatal en FileUploadQueueProcessor.");
            throw;
        }

        _logger.LogInformation("FileUploadQueueProcessor detenido.");
    }

    private async Task ProcessMessageAsync(
        FileUploadMessage fileUpload,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Procesando archivo {FileId} para jersey {JerseyId}",
            fileUpload.IdFile, fileUpload.IdJersey);

        using var scope = _serviceProvider.CreateScope();
        var blobService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();
        var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();

        // Actualizar estado a Processing
        await fileRepository.UpdateProcessingStatusAsync(
            fileUpload.IdFile,
            FileProcessingStatus.Processing,
            cancellationToken: cancellationToken);

        // Verificar que el archivo temporal existe
        if (!File.Exists(fileUpload.TempFilePath))
        {
            throw new FileNotFoundException(
                $"Archivo temporal no encontrado: {fileUpload.TempFilePath}");
        }

        // Determinar el bucket
        var bucketName = fileUpload.ContainerType == "images"
            ? _settings.JerseyImagesBucket
            : _settings.JerseyPatchesBucket;

        // Crear la clave del objeto: jerseyId/uniqueFileName
        var objectKey = $"{fileUpload.IdJersey}/{fileUpload.FileName}";

        // Subir el archivo a Storj
        await using var fileStream = File.OpenRead(fileUpload.TempFilePath);

        var objectUrl = await blobService.UploadAsync(
            bucketName,
            objectKey,
            fileStream,
            fileUpload.ContentType,
            cancellationToken);

        _logger.LogInformation(
            "Archivo {FileId} subido exitosamente a {ObjectUrl}",
            fileUpload.IdFile, objectUrl);

        // Actualizar el registro en la base de datos
        await fileRepository.UpdateProcessingStatusAsync(
            fileUpload.IdFile,
            FileProcessingStatus.Completed,
            url: objectUrl,
            cancellationToken: cancellationToken);

        // Eliminar el archivo temporal
        try
        {
            File.Delete(fileUpload.TempFilePath);
            _logger.LogDebug("Archivo temporal eliminado: {TempPath}", fileUpload.TempFilePath);

            // Intentar eliminar el directorio si está vacío
            var directory = Path.GetDirectoryName(fileUpload.TempFilePath);
            if (directory is not null && Directory.Exists(directory) &&
                !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "No se pudo eliminar el archivo temporal: {TempPath}",
                fileUpload.TempFilePath);
        }

        _logger.LogInformation(
            "Procesamiento completado para archivo {FileId}",
            fileUpload.IdFile);
    }

    private async Task MarkFileAsFailedAsync(
        int fileId,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();

            await fileRepository.UpdateProcessingStatusAsync(
                fileId,
                FileProcessingStatus.Failed,
                errorMessage: errorMessage,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al marcar archivo {FileId} como fallido",
                fileId);
        }
    }
}
