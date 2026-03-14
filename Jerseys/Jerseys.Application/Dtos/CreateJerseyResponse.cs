namespace Jerseys.Application.Dtos;

/// <summary>
/// Respuesta al crear un jersey.
/// </summary>
public sealed record CreateJerseyResponse
{
    /// <summary>
    /// ID del jersey creado.
    /// </summary>
    public required int IdJersey { get; init; }

    /// <summary>
    /// Nombre del jersey.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Estado del procesamiento de imágenes.
    /// </summary>
    public required string ImagesStatus { get; init; }

    /// <summary>
    /// Cantidad de imágenes en cola.
    /// </summary>
    public required int ImagesCount { get; init; }

    /// <summary>
    /// Cantidad de parches en cola.
    /// </summary>
    public required int PatchesCount { get; init; }

    /// <summary>
    /// Mensaje informativo.
    /// </summary>
    public required string Message { get; init; }
}
