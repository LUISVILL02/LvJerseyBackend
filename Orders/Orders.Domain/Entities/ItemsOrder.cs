namespace Orders.Domain.Entities;

public sealed class ItemsOrder
{
    public int IdItemOrder { get; set; }
    public int Quantity { get; set; }
    public string[]? Personalitation { get; set; }
    public string[] Size { get; set; } = null!;
    public int IdJersey { get; set; }
    public int IdOrder { get; set; }
}
