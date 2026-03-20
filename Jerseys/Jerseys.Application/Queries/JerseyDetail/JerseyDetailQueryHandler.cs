using Files.Application.Abstractions;
using Jerseys.Application.Abstractions.Jerseys;
using Microsoft.Extensions.Options;
using Reviews.Application.Abstractions;
using Shared.Application.Abstractions;
using Shared.Application.Settings;
using Shared.Infrastructure.Abstractions;

namespace Jerseys.Application.Queries.JerseyDetail;

public class JerseyDetailQueryHandler(
    IJerseyRepository jerseyRepository,
    IFileRepository fileRepository,
    IReviewRepository reviewRepository,
    IUserContextService userContextService,
    IBlobStorageService blobStorageService,
    IOptions<StorageSettings> storageSettings)
    : IQueryHandler<JerseyDetailQuery, JerseyDetailResponse?>
{
    private const decimal PatchPriceUsd = 1.00m;
    private static readonly string[] StandardSizes = ["S", "M", "L", "XL", "2XL"];

    public async Task<JerseyDetailResponse?> HandleAsync(JerseyDetailQuery query)
    {
        var jersey = await jerseyRepository.GetDetailByIdAsync(query.IdJersey);
        if (jersey is null)
            return null;

        var files = await fileRepository.GetByJerseyIdAsync(query.IdJersey);
        var expiration = TimeSpan.FromMinutes(storageSettings.Value.PresignedUrlExpirationMinutes);
        var expiresAt = DateTime.UtcNow.Add(expiration);

        var imageUrls = await ConvertToPresignedUrlsAsync(
            files.Where(f => !string.IsNullOrEmpty(f.Url) && f.ContainerType!.StartsWith("images")).Select(f => f.Url).ToList(),
            expiration);

        var reviews = await reviewRepository.GetByJerseyIdAsync(query.IdJersey);
        var (avgRating, reviewsCount) = await reviewRepository.GetRatingSummaryAsync(query.IdJersey);

        var userId = userContextService.GetUserId();
        var isFavorite = userId.HasValue && await jerseyRepository.IsFavoriteAsync(query.IdJersey, userId.Value);

        var patchIds = jersey.PatchJerseys.Select(pj => pj.Patch.IdPatch).ToList();
        var patchUrls = patchIds.Count > 0
            ? await fileRepository.GetUrlsByPatchIdsAsync(patchIds)
            : new Dictionary<int, string>();

        var signedPatchUrls = await ConvertToPresignedUrlsAsync(
            patchUrls.Values.Where(v => !string.IsNullOrEmpty(v)).ToList(),
            expiration);

        var patchUrlIndex = 0;
        var patches = jersey.PatchJerseys.Select(pj =>
        {
            var originalUrl = patchUrls.GetValueOrDefault(pj.Patch.IdPatch, string.Empty);
            var signedUrl = !string.IsNullOrEmpty(originalUrl) && patchUrlIndex < signedPatchUrls.Count
                ? signedPatchUrls[patchUrlIndex++]
                : originalUrl;
            return new PatchOptionResponse(
                Id: pj.Patch.IdPatch,
                Name: pj.Patch.NamePatch,
                ImageUrl: signedUrl,
                Price: PatchPriceUsd
            );
        }).ToList();

        var allSizes = await jerseyRepository.GetAllSizesAsync();
        var jerseySizeIds = jersey.SizeJerseys.Select(sj => sj.IdSize).ToHashSet();
        var availableSizes = MapAvailableSizes(allSizes, jerseySizeIds);

        var category = jersey.CategoriesJerseys.FirstOrDefault()?.Category.Name ?? "Sin categoría";
        var description = GenerateDescription(jersey, category);
        var reviewResponses = MapReviews(reviews);

        return new JerseyDetailResponse
        {
            Id = jersey.IdJersey,
            Name = jersey.Name,
            Price = jersey.Price ?? 0m,
            Rating = avgRating,
            ReviewsCount = reviewsCount,
            Category = category,
            Brand = jersey.Brand ?? "Sin marca",
            Type = jersey.Type ?? "Sin tipo",
            Season = jersey.Season ?? "Sin temporada",
            WeightKg = jersey.Weight,
            Tag = jersey.ClubName,
            Images = imageUrls,
            AvailableSizes = availableSizes,
            Patches = patches,
            Stock = jersey.Stock ?? 0,
            Description = description,
            IsFavorite = isFavorite,
            Reviews = reviewResponses,
            ImageUrlExpiresAt = expiresAt
        };
    }

    private async Task<List<string>> ConvertToPresignedUrlsAsync(List<string> publicUrls, TimeSpan expiration)
    {
        var result = new List<string>();

        foreach (var url in publicUrls)
        {
            if (string.IsNullOrEmpty(url))
            {
                result.Add(string.Empty);
                continue;
            }

            var (bucket, objectKey) = ParseStorjUrl(url);
            if (!string.IsNullOrEmpty(bucket) && !string.IsNullOrEmpty(objectKey))
            {
                try
                {
                    result.Add(await blobStorageService.GetPresignedUrlAsync(bucket, objectKey, expiration));
                }
                catch
                {
                    result.Add(url);
                }
            }
            else
            {
                result.Add(url);
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

    private static List<JerseySizeResponse> MapAvailableSizes(
        List<Domain.Entities.Size> allSizes,
        HashSet<int> jerseySizeIds)
    {
        var sizesByName = allSizes.ToDictionary(s => s.NameSize.ToUpperInvariant(), s => s.IdSize);

        return StandardSizes.Select(sizeName =>
        {
            var normalizedName = sizeName.ToUpperInvariant();
            var isAvailable = sizesByName.TryGetValue(normalizedName, out var sizeId)
                              && jerseySizeIds.Contains(sizeId);
            return new JerseySizeResponse(sizeName, isAvailable);
        }).ToList();
    }

    private static string GenerateDescription(Domain.Entities.Jersey jersey, string category)
    {
        var parts = new List<string>
        {
            $"Nombre del producto: {jersey.Name}",
            $"N° del artículo: {jersey.IdJersey}",
            $"Peso: {jersey.Weight} kg",
            $"Categoría: {category}",
            $"Marca: {jersey.Brand ?? "N/A"}",
            $"Tipo: {jersey.Type ?? "N/A"}",
            $"Temporada: {jersey.Season ?? "N/A"}"
        };

        return string.Join("\n", parts);
    }

    private static List<JerseyReviewResponse> MapReviews(List<Reviews.Domain.Entities.Review> reviews)
    {
        return reviews.Select((review, index) =>
        {
            var criteriaByType = review.Criterias.ToDictionary(
                c => c.CriteriaType.ToLowerInvariant(),
                c => c);

            var qualityCriteria = criteriaByType.GetValueOrDefault("quality");
            var deliveryCriteria = criteriaByType.GetValueOrDefault("delivery");
            var detailsCriteria = criteriaByType.GetValueOrDefault("details");

            var overallRating = review.Criterias.Count > 0
                ? review.Criterias.Average(c => c.Value)
                : 0m;

            return new JerseyReviewResponse
            {
                Id = index + 1,
                User = $"Usuario {review.IdUser}",
                CountryFlag = null,
                OverallRating = Math.Round(overallRating, 1),
                Date = review.DateReview?.ToString("yyyy-MM-dd") ?? "Sin fecha",
                Ratings = new ReviewRatingsResponse(
                    Quality: (int)(qualityCriteria?.Value ?? 0),
                    Delivery: (int)(deliveryCriteria?.Value ?? 0),
                    Details: (int)(detailsCriteria?.Value ?? 0)
                ),
                Comments = new ReviewCommentsResponse(
                    Quality: qualityCriteria?.Comment ?? string.Empty,
                    Delivery: deliveryCriteria?.Comment ?? string.Empty,
                    Details: detailsCriteria?.Comment ?? string.Empty
                ),
                GeneralComment = review.GeneralComment ?? string.Empty,
                Images = null
            };
        }).ToList();
    }
}
