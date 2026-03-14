namespace Jerseys.Domain.Entities;

/// <summary>
/// Tabla intermedia para la relación muchos-a-muchos entre Patch y Jersey.
/// </summary>
public sealed class PatchJersey
{
    public int IdPatch { get; init; }
    public int IdJersey { get; init; }

    // Navegaciones
    public Patch Patch { get; init; } = null!;
    public Jersey Jersey { get; init; } = null!;
}
