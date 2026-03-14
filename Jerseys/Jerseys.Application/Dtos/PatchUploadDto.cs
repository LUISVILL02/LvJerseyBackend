namespace Jerseys.Application.Dtos;

/// <summary>
/// DTO para subir un parche con su metadata e imagen.
/// </summary>
public sealed record PatchUploadDto(
    /// <summary>
    /// Nombre del parche (ej: "Champions League").
    /// </summary>
    string Name,

    /// <summary>
    /// Temporada en la que aplica el parche (ej: "24/25").
    /// </summary>
    string Season,

    /// <summary>
    /// Imagen del parche.
    /// </summary>
    ImageUploadDto Image
);
