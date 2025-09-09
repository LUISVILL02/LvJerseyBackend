
namespace Domain.Entities;

public sealed class Address
{
    public int IdAddress { get; init; }

    public string? Neighborhood { get; init; }

    public string? Address1 { get; init; }

    public int IdUser { get; init; }

    public User IdUserNavigation { get; init; } = null!;
}