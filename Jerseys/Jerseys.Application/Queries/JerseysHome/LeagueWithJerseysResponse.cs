using Jerseys.Application.Dtos;

namespace Jerseys.Application.Queries.JerseysHome;

public sealed record LeagueWithJerseysResponse
{
    /// <summary>
    /// Nombre de la liga (La Liga, Premier League, Serie A)
    /// </summary>
    public required string League { get; init; }

    /// <summary>
    /// País de la liga (España, Inglaterra, Italia)
    /// </summary>
    public required string Country { get; init; }

    /// <summary>
    /// Lista de jerseys de la liga
    /// </summary>
    public required List<JerseyCardDto> Jerseys { get; init; }
}

