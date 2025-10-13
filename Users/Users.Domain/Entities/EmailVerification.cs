namespace Domain.Entities;

public class EmailVerification
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required DateTime ExpiryAt { get; set; }
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
}