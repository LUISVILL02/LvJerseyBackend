namespace Jerseys.Domain.Entities;

public sealed class Club
{
    public int IdClub { get; init; }

    public string Name { get; init; } = null!;

    public int IdLeague { get; init; }

    // Navegación
    public League League { get; init; } = null!;
    public ICollection<Jersey> Jerseys { get; init; } = new List<Jersey>();
}

