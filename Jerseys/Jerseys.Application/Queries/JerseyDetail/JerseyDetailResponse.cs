namespace Jerseys.Application.Queries.JerseyDetail;

/// <summary>
/// Respuesta con el detalle completo de un jersey.
/// </summary>
public sealed record JerseyDetailResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required decimal Rating { get; init; }
    public required int ReviewsCount { get; init; }
    public required string Category { get; init; }
    public required string Brand { get; init; }
    public required string Type { get; init; }
    public required string Season { get; init; }
    public required decimal WeightKg { get; init; }
    public string? Tag { get; init; }
    public required IReadOnlyList<string> Images { get; init; }
    public required IReadOnlyList<JerseySizeResponse> AvailableSizes { get; init; }
    public required IReadOnlyList<PatchOptionResponse> Patches { get; init; }
    public required int Stock { get; init; }
    public string? Description { get; init; }
    public required bool IsFavorite { get; init; }
    public IReadOnlyList<JerseyReviewResponse>? Reviews { get; init; }
    public DateTime? ImageUrlExpiresAt { get; init; }
}

/// <summary>
/// Talla disponible para un jersey.
/// </summary>
public sealed record JerseySizeResponse(string Code, bool Available);

/// <summary>
/// Opción de parche para un jersey.
/// </summary>
public sealed record PatchOptionResponse(int Id, string Name, string ImageUrl, decimal Price);

/// <summary>
/// Reseña de un jersey.
/// </summary>
public sealed record JerseyReviewResponse
{
    public required int Id { get; init; }
    public required string User { get; init; }
    public string? CountryFlag { get; init; }
    public required decimal OverallRating { get; init; }
    public required string Date { get; init; }
    public required ReviewRatingsResponse Ratings { get; init; }
    public required ReviewCommentsResponse Comments { get; init; }
    public required string GeneralComment { get; init; }
    public IReadOnlyList<string>? Images { get; init; }
}

/// <summary>
/// Calificaciones desglosadas por criterio.
/// </summary>
public sealed record ReviewRatingsResponse(int Quality, int Delivery, int Details);

/// <summary>
/// Comentarios desglosados por criterio.
/// </summary>
public sealed record ReviewCommentsResponse(string Quality, string Delivery, string Details);
