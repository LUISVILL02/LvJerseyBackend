namespace Reviews.Domain.Entities;

/// <summary>
/// Representa una reseña de un jersey por un usuario.
/// Clave primaria compuesta: (IdJersey, IdUser)
/// </summary>
public sealed class Review
{
    public int IdJersey { get; init; }
    public int IdUser { get; init; }
    public string? GeneralComment { get; set; }
    public DateTime? DateReview { get; set; }

    // Navegación
    public ICollection<Criteria> Criterias { get; init; } = new List<Criteria>();
}
