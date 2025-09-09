
namespace Domain.Entities;

public sealed class User
{
    public int IdUser { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Names { get; set; }

    public string? LastName { get; set; }

    public string? Nikname { get; set; }

    public string? Country { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public int? PostalCode { get; set; }

    public int IdRol { get; set; }

    public ICollection<Address> Adresses { get; set; } = new List<Address>();

    public Role IdRolNavigation { get; set; } = null!;
}