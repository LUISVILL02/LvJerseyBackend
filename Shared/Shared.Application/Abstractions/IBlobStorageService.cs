namespace Shared.Application.Abstractions;

/// <summary>
/// Interface genérica para servicios de almacenamiento de blobs/objetos.
/// Permite abstraer el proveedor de almacenamiento (Storj, S3, Azure, etc.)
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Sube un archivo al bucket especificado.
    /// </summary>
    /// <param name="bucketName">Nombre del bucket (jersey-images o jersey-patches)</param>
    /// <param name="objectKey">Clave del objeto (ruta dentro del bucket)</param>
    /// <param name="content">Stream del contenido del archivo</param>
    /// <param name="contentType">Tipo MIME del archivo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>URL pública del objeto subido</returns>
    Task<string> UploadAsync(
        string bucketName,
        string objectKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un objeto del bucket.
    /// </summary>
    /// <param name="bucketName">Nombre del bucket</param>
    /// <param name="objectKey">Clave del objeto a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task DeleteAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la URL de un objeto existente.
    /// </summary>
    /// <param name="bucketName">Nombre del bucket</param>
    /// <param name="objectKey">Clave del objeto</param>
    /// <returns>URL del objeto</returns>
    string GetObjectUrl(string bucketName, string objectKey);

    /// <summary>
    /// Verifica si un objeto existe.
    /// </summary>
    /// <param name="bucketName">Nombre del bucket</param>
    /// <param name="objectKey">Clave del objeto</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe, false en caso contrario</returns>
    Task<bool> ExistsAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken = default);
}
