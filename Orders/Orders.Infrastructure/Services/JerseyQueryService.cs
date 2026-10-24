using Files.Application.Abstractions;
using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Shared.Infrastructure.Data;

namespace Orders.Infrastructure.Services;

public class JerseyQueryService(ApplicationDbContext context, IFileRepository fileRepository) : IJerseyQueryService
{
    public async Task<JerseyInfoDto?> GetByIdAsync(int idJersey, CancellationToken cancellationToken = default)
    {
        var jersey = await context.Set<Jersey>()
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdJersey == idJersey, cancellationToken);

        var jerseyImg = await fileRepository.GetByJerseyIdAsync(idJersey, cancellationToken)
            ?? throw new InvalidOperationException($"Image for jersey with ID {idJersey} not found");

        if (jersey is null) return null;

        return new JerseyInfoDto(
            IdJersey: jersey.IdJersey,
            Name: jersey.Name,
            Price: jersey.Price ?? 0,
            Weight: jersey.Weight,
            Stock: jersey.Stock ?? 0,
            ImageUrl: jerseyImg.FirstOrDefault()?.Url ?? string.Empty
        );
    }
}
