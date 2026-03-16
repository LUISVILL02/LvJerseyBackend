namespace Jerseys.Domain.Entities;

public sealed class Jersey
{
    public int IdJersey { get; init; }

    public string Name { get; init; } = null!;

    public decimal Weight { get; init; }

    /// <summary>
    /// Valores esperados: Player, Fan, Retro
    /// </summary>
    public string? Type { get; init; }

    public string? Brand { get; init; }

    public string? Season { get; init; }

    public decimal? Price { get; init; }

    public string? Sex { get; init; }

    public int? Stock { get; init; }

    public int IdClub { get; init; }

    /// <summary>
    /// Nombre del club (desnormalizado para consultas rápidas).
    /// </summary>
    public string? ClubName { get; init; }

    // Navegación
    public Club ClubNavigation { get; init; } = null!;
    public ICollection<Favorite> FavoriteJerseys { get; init; } = new List<Favorite>();
    public ICollection<CategoriesJersey> CategoriesJerseys { get; init; } = new List<CategoriesJersey>();
    public ICollection<PatchJersey> PatchJerseys { get; init; } = new List<PatchJersey>();
    public ICollection<SizeJersey> SizeJerseys { get; init; } = new List<SizeJersey>();
}

