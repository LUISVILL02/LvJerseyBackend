using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Queries.JerseysHome;
using Jerseys.Application.Dtos;
using Jerseys.Domain.Entities;
using Shared.Infrastructure.Data;
using Shared.Application.Abstractions;
using Shared.Application.Settings;
using Files.Application.Abstractions;

namespace Jerseys.Infrastructure.Services;

public class JerseyRepository(
    ApplicationDbContext context,
    IFileRepository fileRepository,
    IBlobStorageService blobStorageService,
    IOptions<StorageSettings> storageSettings) : IJerseyRepository
{
    private static readonly string[] JerseyBuckets = ["jersey-images", "jersey-patches"];

    public async Task<List<LeagueWithJerseysResponse>> GetJerseysByCategoryAsync(int? idUser)
    {
        var jerseysEntities = await context.Set<Jersey>()
            .Include(j => j.ClubNavigation)
                .ThenInclude(c => c.League)
            .AsNoTracking()
            .ToListAsync();

        var jerseyIds = jerseysEntities.Select(j => j.IdJersey).ToList();

        var imageUrls = await fileRepository.GetFirstImageUrlsByJerseyIdsAsync(jerseyIds);
        var signedImageUrls = await ConvertToPresignedUrlsAsync(imageUrls);

        var grouped = jerseysEntities
            .GroupBy(j => new { j.ClubNavigation.League!.IdLeague, j.ClubNavigation.League.Name, j.ClubNavigation.League.Country })
            .ToList();

        var favorites = await context.Set<Favorite>()
            .Where(f => f.IdUser == idUser)
            .AsNoTracking()
            .ToListAsync();

        var expirationMinutes = storageSettings.Value.PresignedUrlExpirationMinutes;
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var result = grouped.Select(g => new LeagueWithJerseysResponse
        {
            League = g.Key.Name,
            Country = g.Key.Country,
            Jerseys = g.Select(j => new JerseyCardDto
            {
                Id = j.IdJersey,
                ImageUrl = signedImageUrls.GetValueOrDefault(j.IdJersey, string.Empty),
                Name = j.Name,
                TypeDescription = j.Type,
                Price = j.Price ?? 0m,
                Rating = 0m,
                IsFavorite = favorites.Any(f => f.idJersey == j.IdJersey),
                ImageUrlExpiresAt = expiresAt
            }).ToList()
        }).ToList();

        return result;
    }

    private async Task<Dictionary<int, string>> ConvertToPresignedUrlsAsync(Dictionary<int, string> publicUrls)
    {
        var result = new Dictionary<int, string>();
        var expiration = TimeSpan.FromMinutes(storageSettings.Value.PresignedUrlExpirationMinutes);

        foreach (var (jerseyId, publicUrl) in publicUrls)
        {
            if (string.IsNullOrEmpty(publicUrl))
            {
                result[jerseyId] = string.Empty;
                continue;
            }

            var (bucket, objectKey) = ParseStorjUrl(publicUrl);
            if (!string.IsNullOrEmpty(bucket) && !string.IsNullOrEmpty(objectKey))
            {
                try
                {
                    result[jerseyId] = await blobStorageService.GetPresignedUrlAsync(bucket, objectKey, expiration);
                }
                catch
                {
                    result[jerseyId] = publicUrl;
                }
            }
            else
            {
                result[jerseyId] = publicUrl;
            }
        }

        return result;
    }

    private static (string? Bucket, string? ObjectKey) ParseStorjUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return (null, null);

        try
        {
            var uri = new Uri(url);
            var segments = uri.Segments.Where(s => s != "/").ToArray();

            if (segments.Length >= 2)
            {
                var bucket = Uri.UnescapeDataString(segments[0]).TrimEnd('/');
                var objectKey = string.Join("", segments.Skip(1).Select(s => Uri.UnescapeDataString(s)));
                return (bucket, objectKey);
            }
        }
        catch
        {
        }

        return (null, null);
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

    public async Task<(int IdClub, string ClubName)> GetOrCreateClubAsync(
        string clubName,
        IReadOnlyCollection<string> categories,
        CancellationToken cancellationToken = default)
    {
        var existingClub = await context.Set<Club>()
            .FirstOrDefaultAsync(c => c.Name == clubName, cancellationToken);

        if (existingClub != null)
            return (existingClub.IdClub, existingClub.Name);

        var league = await context.Set<League>()
            .FirstOrDefaultAsync(l => categories.Contains(l.Name), cancellationToken);

        if (league == null)
            throw new InvalidOperationException(
                $"No se encontró una liga válida en las categorías. El club '{clubName}' no puede crearse sin liga.");

        var newClub = new Club { Name = clubName, IdLeague = league.IdLeague };
        context.Set<Club>().Add(newClub);
        await context.SaveChangesAsync(cancellationToken);

        return (newClub.IdClub, newClub.Name);
    }

    public async Task<List<int>> GetSizeIdsBySymbolsAsync(
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken = default)
    {
        var sizes = await context.Set<Size>()
            .Where(s => symbols.Contains(s.NameSize))
            .ToListAsync(cancellationToken);

        var missing = symbols.Except(sizes.Select(s => s.NameSize)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Los siguientes símbolos de talla no existen: {string.Join(", ", missing)}");

        return sizes.Select(s => s.IdSize).ToList();
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
