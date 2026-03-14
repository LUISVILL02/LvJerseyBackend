using Files.Domain.Enums;

namespace Files.Domain.Entities;

public sealed class File
{
    public int IdFile { get; set; }

    public string Url { get; set; } = null!;

    public string? Name { get; set; }

    public string[]? Format { get; set; }

    public int? IdJersey { get; set; }

    public int? IdUser { get; set; }

    /// <summary>
    /// Estado del procesamiento del archivo en Azure Blob Storage.
    /// </summary>
    public FileProcessingStatus ProcessingStatus { get; set; } = FileProcessingStatus.Pending;

    /// <summary>
    /// Tipo de contenedor destino: "images" o "patches".
    /// </summary>
    public string? ContainerType { get; set; }

    /// <summary>
    /// Ruta temporal del archivo antes de ser subido a Azure Blob.
    /// </summary>
    public string? TempFilePath { get; set; }

    /// <summary>
    /// Tipo MIME del archivo (image/jpeg, image/png, etc.).
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Mensaje de error si el procesamiento falló.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public ICollection<FileJersey> FileJerseys { get; set; } = new List<FileJersey>();
    public ICollection<FilePatch> FilePatches { get; set; } = new List<FilePatch>();
}
