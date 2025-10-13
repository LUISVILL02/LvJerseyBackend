
namespace Domain.Entities;

public sealed class User
{
    public int IdUser { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Names { get; set; }

    public string? LastName { get; set; }

    public string? Nickname { get; set; }

    public string? Country { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public int? PostalCode { get; set; }

    public int IdRol { get; set; }
    
    public string? RefreshToken { get; set;  }
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    public bool EmailConfirmed { get; set; }

    public ICollection<Address>? Adresses { get; set; } = new List<Address>();
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();

    public Role Role { get; set; } = null!;
}