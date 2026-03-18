namespace Jerseys.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación muchos-a-muchos entre Size y Jersey.
/// </summary>
public sealed class SizeJersey
{
    public int IdJersey { get; init; }
    public int IdSize { get; init; }

    // Navegación
    public Jersey Jersey { get; init; } = null!;
    public Size Size { get; init; } = null!;
}
