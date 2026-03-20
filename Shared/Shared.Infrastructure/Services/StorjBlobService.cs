using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions;
using Shared.Application.Settings;

namespace Shared.Infrastructure.Services;

public sealed class StorjBlobService : IBlobStorageService, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageSettings _settings;
    private readonly ILogger<StorjBlobService> _logger;
    private readonly HashSet<string> _ensuredBuckets = [];

    public StorjBlobService(
        IOptions<StorageSettings> settings,
        ILogger<StorjBlobService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        AWSConfigsS3.UseSignatureVersion4 = true;

        var config = new AmazonS3Config
        {
            ServiceURL = _settings.ServiceUrl,
            ForcePathStyle = true,
            UseHttp = _settings.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
        };

        _s3Client = new AmazonS3Client(
            _settings.AccessKey,
            _settings.SecretKey,
            config);

        _logger.LogInformation(
            "StorjBlobService inicializado con endpoint: {Endpoint}",
            _settings.ServiceUrl);
    }

    public async Task<string> UploadAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucketName, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType,
            // Hacer el objeto públicamente accesible
            CannedACL = S3CannedACL.PublicRead
        };

        _logger.LogDebug(
            "Subiendo objeto {ObjectKey} al bucket {BucketName}",
            objectKey, bucketName);

        await _s3Client.PutObjectAsync(request, cancellationToken);

        var url = GetObjectUrl(bucketName, objectKey);

        _logger.LogInformation(
            "Objeto subido exitosamente: {Url}",
            url);

        return url;
    }

    public async Task DeleteAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };

        _logger.LogDebug(
            "Eliminando objeto {ObjectKey} del bucket {BucketName}",
            objectKey, bucketName);

        await _s3Client.DeleteObjectAsync(request, cancellationToken);

        _logger.LogInformation(
            "Objeto eliminado: {BucketName}/{ObjectKey}",
            bucketName, objectKey);
    }

    public string GetObjectUrl(string bucketName, string objectKey)
    {
        // Construir URL pública para Storj
        // Formato: https://link.storjshare.io/s/{bucketName}/{objectKey}
        // O usando el gateway: {ServiceUrl}/{bucketName}/{objectKey}
        var baseUrl = _settings.ServiceUrl.TrimEnd('/');
        return $"{baseUrl}/{bucketName}/{objectKey}";
    }

    public async Task<bool> ExistsAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.Add(expiration),
            Verb = HttpVerb.GET
        };

        _logger.LogDebug(
            "Generando URL firmada para {BucketName}/{ObjectKey} con expiración {Expiration} minutos",
            bucketName, objectKey, expiration.TotalMinutes);

        var url = await _s3Client.GetPreSignedURLAsync(request);

        _logger.LogDebug(
            "URL firmada generada: {Url}",
            url);

        return url;
    }

    private async Task EnsureBucketExistsAsync(
        string bucketName,
        CancellationToken cancellationToken)
    {
        // Evitar verificar el mismo bucket múltiples veces
        if (_ensuredBuckets.Contains(bucketName))
            return;

        try
        {
            // Verificar si el bucket existe
            await _s3Client.GetBucketLocationAsync(bucketName, cancellationToken);
            _ensuredBuckets.Add(bucketName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Crear el bucket si no existe
            _logger.LogInformation("Creando bucket: {BucketName}", bucketName);

            var request = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            await _s3Client.PutBucketAsync(request, cancellationToken);
            _ensuredBuckets.Add(bucketName);

            _logger.LogInformation("Bucket creado: {BucketName}", bucketName);
        }
    }

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}
