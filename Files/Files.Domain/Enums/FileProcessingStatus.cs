namespace Files.Domain.Enums;

/// <summary>
/// Estado del procesamiento de un archivo en Azure Blob Storage.
/// </summary>
public enum FileProcessingStatus
{
    /// <summary>
    /// El archivo está pendiente de ser subido a Azure Blob.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// El archivo se está procesando (subiendo a Azure Blob).
    /// </summary>
    Processing = 1,

    /// <summary>
    /// El archivo fue subido exitosamente a Azure Blob.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Hubo un error al subir el archivo a Azure Blob.
    /// </summary>
    Failed = 3
}
