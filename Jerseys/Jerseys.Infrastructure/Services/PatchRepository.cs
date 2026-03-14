using Jerseys.Application.Abstractions.Patches;
using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Jerseys.Infrastructure.Services;

public class PatchRepository(ApplicationDbContext context) : IPatchRepository
{
    public async Task<Patch> CreateAsync(Patch patch, CancellationToken cancellationToken = default)
    {
        context.Set<Patch>().Add(patch);
        await context.SaveChangesAsync(cancellationToken);
        return patch;
    }

    public async Task CreatePatchJerseyAsync(int idPatch, int idJersey, CancellationToken cancellationToken = default)
    {
        var patchJersey = new PatchJersey
        {
            IdPatch = idPatch,
            IdJersey = idJersey
        };

        context.Set<PatchJersey>().Add(patchJersey);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Patch?> GetByIdAsync(int idPatch, CancellationToken cancellationToken = default)
    {
        return await context.Set<Patch>()
            .FirstOrDefaultAsync(p => p.IdPatch == idPatch, cancellationToken);
    }
}
