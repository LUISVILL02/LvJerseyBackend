
namespace Domain.Entities;

public sealed class Role
{
    public int IdRol { get; set; }

    public string? Name { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}