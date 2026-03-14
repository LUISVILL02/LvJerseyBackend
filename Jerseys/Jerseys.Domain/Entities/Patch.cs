namespace Jerseys.Domain.Entities;

/// <summary>
/// Representa un parche/badge que puede aplicarse a jerseys.
/// Ejemplos: Champions League, La Liga, Copa del Rey, etc.
/// </summary>
public sealed class Patch
{
    public int IdPatch { get; init; }

    /// <summary>
    /// Nombre del parche (ej: "Champions League", "La Liga").
    /// </summary>
    public string NamePatch { get; init; } = null!;

    /// <summary>
    /// Temporada en la que aplica el parche (ej: "24/25").
    /// </summary>
    public string Season { get; init; } = null!;

    // Navegaciones
    public ICollection<PatchJersey> PatchJerseys { get; init; } = new List<PatchJersey>();
}
