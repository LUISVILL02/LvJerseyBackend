namespace Orders.Domain.Entities;

public sealed class Order
{
    public int IdOrder { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalWeight { get; set; }
    public string Email { get; set; } = null!;
    public int IdUser { get; set; }
    public string AddressId { get; set; } = null!;
    public decimal ShippingCost { get; set; }
    public string Status { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public string PaymentAccountId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<ItemsOrder> ItemsOrders { get; set; } = new List<ItemsOrder>();
}
