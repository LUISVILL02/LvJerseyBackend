namespace Shared.Application.Settings;

/// <summary>
/// Configuración para el servicio de almacenamiento de archivos (Storj S3-compatible).
/// </summary>
public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Access Key para autenticación S3 (Storj Access Grant).
    /// </summary>
    public string AccessKey { get; set; } = null!;

    /// <summary>
    /// Secret Key para autenticación S3 (Storj Secret Key).
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// URL del endpoint S3-compatible de Storj.
    /// </summary>
    public string ServiceUrl { get; set; } = "https://gateway.storjshare.io";

    /// <summary>
    /// Nombre del bucket para imágenes de jerseys.
    /// </summary>
    public string JerseyImagesBucket { get; set; } = "jersey-images";

    /// <summary>
    /// Nombre del bucket para patches de jerseys.
    /// </summary>
    public string JerseyPatchesBucket { get; set; } = "jersey-patches";

    /// <summary>
    /// Tamaño máximo de archivo en bytes (default: 5MB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// Extensiones de archivo permitidas.
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>
    /// Tiempo de expiración para URLs firmadas en minutos (default: 60).
    /// </summary>
    public int PresignedUrlExpirationMinutes { get; set; } = 60;
}
