using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Jerseys.Infrastructure.Services;

public class LeagueRepository(ApplicationDbContext context) : ILeagueRepository
{
    public async Task<League?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Set<League>()
            .FirstOrDefaultAsync(l => l.Name == name, cancellationToken);
    }
}
