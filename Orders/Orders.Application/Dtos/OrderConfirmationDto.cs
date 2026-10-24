namespace Orders.Application.Dtos;

public sealed record OrderConfirmationDto(
    string OrderNumber,
    string Date,
    List<CartItemDto> Items,
    int TotalItems,
    decimal TotalPrice,
    decimal ShippingCost,
    string PaymentMethod,
    string PaymentStatus,
    string Status
);

public sealed record CartItemDto(
    int JerseyId,
    string Name,
    int Quantity,
    decimal Price,
    decimal WeightKg,
    string Size,
    List<ItemPatchDto>? Patches,
    CustomizationDto? Customization,
    string? ImageUrl
);

public sealed record ItemPatchDto(string Name, decimal Price);
public sealed record CustomizationDto(string? Name, string? Number);
