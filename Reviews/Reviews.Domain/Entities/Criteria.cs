namespace Reviews.Domain.Entities;

/// <summary>
/// Representa un criterio de evaluación de una reseña.
/// Cada reseña tiene 3 criterios: quality, delivery, details.
/// </summary>
public sealed class Criteria
{
    public int IdCriteria { get; init; }
    
    /// <summary>
    /// Tipo de criterio: "quality", "delivery", "details"
    /// </summary>
    public string CriteriaType { get; set; } = null!;
    
    /// <summary>
    /// Valor de la calificación (0-5)
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// Comentario opcional sobre el criterio
    /// </summary>
    public string? Comment { get; set; }
    
    public int IdJersey { get; init; }
    public int IdUser { get; init; }

    // Navegación
    public Review Review { get; init; } = null!;
}
