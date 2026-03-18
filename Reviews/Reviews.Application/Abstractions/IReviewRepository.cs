using Reviews.Domain.Entities;

namespace Reviews.Application.Abstractions;

public interface IReviewRepository
{
    /// <summary>
    /// Obtiene todas las reseñas de un jersey con sus criterios.
    /// </summary>
    Task<List<Review>> GetByJerseyIdAsync(int idJersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el promedio de calificación y la cantidad de reseñas de un jersey.
    /// </summary>
    Task<(decimal avgRating, int count)> GetRatingSummaryAsync(int idJersey, CancellationToken cancellationToken = default);
}
