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

    // Navegación
    public Club Club { get; init; } = null!;
    public ICollection<Favorite> FavoriteJerseys { get; init; } = new List<Favorite>();
}

