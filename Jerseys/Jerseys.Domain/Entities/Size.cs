namespace Jerseys.Domain.Entities;

/// <summary>
/// Representa una talla disponible para jerseys.
/// Ejemplos: S, M, L, XL, 2XL
/// </summary>
public sealed class Size
{
    public int IdSize { get; init; }
    public string NameSize { get; init; } = null!;

    // Navegación
    public ICollection<SizeJersey> SizeJerseys { get; init; } = new List<SizeJersey>();
}
