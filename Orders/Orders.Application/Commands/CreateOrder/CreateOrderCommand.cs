using Orders.Application.Dtos;
using Shared.Application.Abstractions;

namespace Orders.Application.Commands.CreateOrder;

using Orders.Application.Dtos;

public sealed record PatchDto(string Name, decimal Price);
public sealed record PaymentDto(string Method, string AccountId);

public sealed record CreateOrderItemDto(
    int JerseyId,
    string Size,
    string Name,
    decimal Price,
    int Quantity,
    decimal WeightKg,
    List<PatchDto>? Patches,
    CustomizationDto? Customization
);

public sealed record CreateOrderCommand(
    string IdAddress,
    List<CreateOrderItemDto> Items,
    PaymentDto Payment,
    decimal ShippingCost
) : ICommand<OrderConfirmationDto>;
