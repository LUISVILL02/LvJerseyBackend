namespace Jerseys.Domain.Entities;

public sealed class League
{
    public int IdLeague { get; init; }

    public string Name { get; init; } = null!;

    public string Country { get; init; } = null!;

    public ICollection<Club> Clubs { get; init; } = new List<Club>();
}

