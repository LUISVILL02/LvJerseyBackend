namespace Jerseys.Domain.Entities;

public sealed class Favorite
{
    public int idJersey { get; set; }
    
    public Jersey Jersey { get; init; } = null!;
    public int IdUser { get; set; }
}