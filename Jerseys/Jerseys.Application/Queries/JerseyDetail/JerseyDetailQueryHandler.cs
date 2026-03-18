using Files.Application.Abstractions;
using Jerseys.Application.Abstractions.Jerseys;
using Reviews.Application.Abstractions;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Abstractions;

namespace Jerseys.Application.Queries.JerseyDetail;

public class JerseyDetailQueryHandler(
    IJerseyRepository jerseyRepository,
    IFileRepository fileRepository,
    IReviewRepository reviewRepository,
    IUserContextService userContextService)
    : IQueryHandler<JerseyDetailQuery, JerseyDetailResponse?>
{
    /// <summary>
    /// Precio constante de cada parche en USD.
    /// </summary>
    private const decimal PatchPriceUsd = 1.00m;

    /// <summary>
    /// Tallas estándar disponibles.
    /// </summary>
    private static readonly string[] StandardSizes = ["S", "M", "L", "XL", "2XL"];

    public async Task<JerseyDetailResponse?> HandleAsync(JerseyDetailQuery query)
    {
        // 1. Obtener el jersey con todas sus relaciones
        var jersey = await jerseyRepository.GetDetailByIdAsync(query.IdJersey);
        if (jersey is null)
            return null;

        // 2. Obtener las imágenes del jersey
        var files = await fileRepository.GetByJerseyIdAsync(query.IdJersey);
        var imageUrls = files
            .Where(f => !string.IsNullOrEmpty(f.Url))
            .Select(f => f.Url)
            .ToList();

        // 3. Obtener las reseñas y el rating
        var reviews = await reviewRepository.GetByJerseyIdAsync(query.IdJersey);
        var (avgRating, reviewsCount) = await reviewRepository.GetRatingSummaryAsync(query.IdJersey);

        // 4. Verificar si es favorito del usuario actual
        var userId = userContextService.GetUserId();
        var isFavorite = userId.HasValue && await jerseyRepository.IsFavoriteAsync(query.IdJersey, userId.Value);

        // 5. Obtener las URLs de las imágenes de los patches
        var patchIds = jersey.PatchJerseys.Select(pj => pj.Patch.IdPatch).ToList();
        var patchImageUrls = patchIds.Count > 0
            ? await fileRepository.GetUrlsByPatchIdsAsync(patchIds)
            : new Dictionary<int, string>();

        // 6. Obtener todas las tallas disponibles del sistema
        var allSizes = await jerseyRepository.GetAllSizesAsync();
        var jerseySizeIds = jersey.SizeJerseys.Select(sj => sj.IdSize).ToHashSet();

        // 7. Mapear tallas: mostrar las tallas estándar y marcar disponibilidad
        var availableSizes = MapAvailableSizes(allSizes, jerseySizeIds);

        // 8. Obtener la primera categoría (o "Sin categoría")
        var category = jersey.CategoriesJerseys.FirstOrDefault()?.Category.Name ?? "Sin categoría";

        // 9. Generar la descripción del jersey
        var description = GenerateDescription(jersey, category);

        // 10. Mapear patches
        var patches = jersey.PatchJerseys.Select(pj => new PatchOptionResponse(
            Id: pj.Patch.IdPatch,
            Name: pj.Patch.NamePatch,
            ImageUrl: patchImageUrls.GetValueOrDefault(pj.Patch.IdPatch, string.Empty),
            Price: PatchPriceUsd
        )).ToList();

        // 11. Mapear reseñas
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
            Reviews = reviewResponses
        };
    }

    private static List<JerseySizeResponse> MapAvailableSizes(
        List<Domain.Entities.Size> allSizes,
        HashSet<int> jerseySizeIds)
    {
        // Crear un diccionario de tallas del sistema por nombre
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
            // Obtener criterios por tipo
            var criteriaByType = review.Criterias.ToDictionary(
                c => c.CriteriaType.ToLowerInvariant(),
                c => c);

            var qualityCriteria = criteriaByType.GetValueOrDefault("quality");
            var deliveryCriteria = criteriaByType.GetValueOrDefault("delivery");
            var detailsCriteria = criteriaByType.GetValueOrDefault("details");

            // Calcular rating general como promedio de los 3 criterios
            var overallRating = review.Criterias.Count > 0
                ? review.Criterias.Average(c => c.Value)
                : 0m;

            return new JerseyReviewResponse
            {
                Id = index + 1, // Usamos índice como ID ya que Review tiene PK compuesta
                User = $"Usuario {review.IdUser}",
                CountryFlag = null, // No tenemos esta información actualmente
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
                Images = null // No tenemos imágenes de reseñas actualmente
            };
        }).ToList();
    }
}
