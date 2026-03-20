using Jerseys.Domain.Entities;

namespace Jerseys.Application.Abstractions.Jerseys;

public interface ILeagueRepository
{
    Task<League?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
