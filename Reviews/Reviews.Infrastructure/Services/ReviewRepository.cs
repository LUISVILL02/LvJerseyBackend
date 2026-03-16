using Microsoft.EntityFrameworkCore;
using Reviews.Application.Abstractions;
using Reviews.Domain.Entities;
using Shared.Infrastructure.Data;

namespace Reviews.Infrastructure.Services;

public class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public async Task<List<Review>> GetByJerseyIdAsync(int idJersey, CancellationToken cancellationToken = default)
    {
        return await context.Set<Review>()
            .Include(r => r.Criterias)
            .Where(r => r.IdJersey == idJersey)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(decimal avgRating, int count)> GetRatingSummaryAsync(int idJersey, CancellationToken cancellationToken = default)
    {
        var reviews = await context.Set<Review>()
            .Include(r => r.Criterias)
            .Where(r => r.IdJersey == idJersey)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
            return (0m, 0);

        // Calcular el promedio general: promedio de los 3 criterios por cada review
        var totalAvg = reviews
            .Where(r => r.Criterias.Count > 0)
            .Select(r => r.Criterias.Average(c => c.Value))
            .DefaultIfEmpty(0m)
            .Average();

        return (Math.Round(totalAvg, 1), reviews.Count);
    }
}
