namespace Jerseys.Domain.Entities;

public sealed class CategoriesJersey
{
    public int IdCategory { get; init; }
    public int IdJersey { get; init; }

    // Navegación
    public Category Category { get; init; } = null!;
    public Jersey Jersey { get; init; } = null!;
}


