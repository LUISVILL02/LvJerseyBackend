using Microsoft.EntityFrameworkCore;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Queries.JerseysHome;
using Jerseys.Application.Dtos;
using Jerseys.Domain.Entities;
using Shared.Infrastructure.Data;

namespace Jerseys.Infrastructure.Services;

public class JerseyRepository(ApplicationDbContext context) : IJerseyRepository
{
    public async Task<List<LeagueWithJerseysResponse>> GetJerseysByCategoryAsync(int? idUser)
    {
        // Cargar jerseys con sus relaciones necesarias
        var jerseysEntities = await context.Set<Jersey>()
            .Include(j => j.ClubNavigation)
                .ThenInclude(c => c.League)
            .AsNoTracking()
            .ToListAsync();

        // Agrupar por liga usando una clave compuesta (Id, Name, Country)
        var grouped = jerseysEntities
            .GroupBy(j => new { j.ClubNavigation.League.IdLeague, j.ClubNavigation.League.Name, j.ClubNavigation.League.Country })
            .ToList();

        // Obtener favoritos
        var favorites = await context.Set<Favorite>()
            .Where(f => f.IdUser == idUser)
            .AsNoTracking()
            .ToListAsync();

        var result = grouped.Select(g => new LeagueWithJerseysResponse
        {
            League = g.Key.Name,
            Country = g.Key.Country,
            Jerseys = g.Select(j => new JerseyCardDto
            {
                Id = j.IdJersey,
                ImageUrl = string.Empty, // Se llenará cuando se consulten los archivos
                Name = j.Name,
                TypeDescription = j.Type,
                Price = j.Price ?? 0m,
                Rating = 0m,
                IsFavorite = favorites.Any(f => f.idJersey == j.IdJersey),
            }).ToList()
        }).ToList();

        return result;
    }

    public async Task<Jersey> CreateAsync(Jersey jersey, CancellationToken cancellationToken = default)
    {
        context.Set<Jersey>().Add(jersey);
        await context.SaveChangesAsync(cancellationToken);
        return jersey;
    }

    public async Task<Jersey?> GetByIdAsync(int idJersey, CancellationToken cancellationToken = default)
    {
        return await context.Set<Jersey>()
            .Include(j => j.ClubNavigation)
                .ThenInclude(c => c.League)
            .FirstOrDefaultAsync(j => j.IdJersey == idJersey, cancellationToken);
    }

    public async Task<bool> ClubExistsAsync(int idClub, CancellationToken cancellationToken = default)
    {
        return await context.Set<Club>()
            .AnyAsync(c => c.IdClub == idClub, cancellationToken);
    }

    public async Task<string?> GetClubNameAsync(int idClub, CancellationToken cancellationToken = default)
    {
        return await context.Set<Club>()
            .Where(c => c.IdClub == idClub)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CreateSizeJerseysAsync(int idJersey, IEnumerable<int> sizeIds, CancellationToken cancellationToken = default)
    {
        var sizeJerseys = sizeIds.Select(sizeId => new SizeJersey
        {
            IdJersey = idJersey,
            IdSize = sizeId
        });

        context.Set<SizeJersey>().AddRange(sizeJerseys);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Jersey?> GetDetailByIdAsync(int idJersey, CancellationToken cancellationToken = default)
    {
        return await context.Set<Jersey>()
            .Include(j => j.ClubNavigation)
                .ThenInclude(c => c.League)
            .Include(j => j.CategoriesJerseys)
                .ThenInclude(cj => cj.Category)
            .Include(j => j.SizeJerseys)
                .ThenInclude(sj => sj.Size)
            .Include(j => j.PatchJerseys)
                .ThenInclude(pj => pj.Patch)
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.IdJersey == idJersey, cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(int idJersey, int idUser, CancellationToken cancellationToken = default)
    {
        return await context.Set<Favorite>()
            .AnyAsync(f => f.idJersey == idJersey && f.IdUser == idUser, cancellationToken);
    }

    public async Task<List<Size>> GetAllSizesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<Size>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
