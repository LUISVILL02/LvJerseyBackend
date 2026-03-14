namespace Jerseys.Application.Dtos;

public sealed record JerseyCardDto
{
    /// <summary>
    /// Identificador único del jersey
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// URL de la imagen del jersey
    /// </summary>
    public required string ImageUrl { get; init; }

    /// <summary>
    /// Nombre del jersey (ej: Camiseta Local Real Madrid 23/24)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Descripción del tipo: Versión jugador, fanático, retro
    /// </summary>
    public string? TypeDescription { get; init; }

    /// <summary>
    /// Precio del jersey
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Calificación del jersey (0-5)
    /// </summary>
    public required decimal Rating { get; init; }

    /// <summary>
    /// Indica si el jersey está marcado como favorito
    /// </summary>
    public required bool IsFavorite { get; init; }
}
