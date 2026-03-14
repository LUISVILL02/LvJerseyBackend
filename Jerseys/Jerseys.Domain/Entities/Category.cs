namespace Jerseys.Domain.Entities;

public sealed class Category
{
    public int IdCategory { get; init; }

    public string Name { get; init; } = null!;

    // Navegación
    public ICollection<CategoriesJersey> CategoriesJerseys { get; init; } = new List<CategoriesJersey>();
}

