namespace Shared.Application.Dtos;

/// <summary>
/// Mensaje que se envía a la cola para procesar la subida de archivos a Storj.
/// </summary>
public sealed record FileUploadMessage
{
    /// <summary>
    /// ID del registro File en la base de datos.
    /// </summary>
    public required int IdFile { get; init; }

    /// <summary>
    /// Ruta temporal donde se guardó el archivo.
    /// </summary>
    public required string TempFilePath { get; init; }

    /// <summary>
    /// Nombre original del archivo.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Tipo MIME del archivo (image/jpeg, image/png, etc.).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Tipo de contenedor destino: "images" o "patches".
    /// </summary>
    public required string ContainerType { get; init; }

    /// <summary>
    /// Fecha en que se encoló el mensaje.
    /// </summary>
    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
}
