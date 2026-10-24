using Microsoft.Extensions.Logging;
using Orders.Application.Abstractions;
using Orders.Application.Dtos;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Payments.Application.Abstractions;
using Shared.Application.Abstractions;

namespace Orders.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    IJerseyQueryService jerseyQueryService,
    IUserContextService userContextService,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, OrderConfirmationDto>
{
    private const decimal PatchPriceUsd = 5000m;

    public async Task<OrderConfirmationDto> HandleAsync(CreateOrderCommand command)
    {
        var userId = userContextService.GetUserId()
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var email = userContextService.GetUserEmail() ?? throw new UnauthorizedAccessException("Email not found");

        // Validad la dirección del pedido a ver si existe y corresponde al cliente 

        var items = new List<ItemsOrder>();
        var cartItems = new List<CartItemDto>();
        var totalItems = 0;
        var totalWeight = 0m;
        var subtotal = 0m;

        foreach (var item in command.Items)
        {
            var jersey = await jerseyQueryService.GetByIdAsync(item.JerseyId)
                ?? throw new InvalidOperationException($"Jersey with ID {item.JerseyId} not found");

            if (jersey.Stock < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for jersey {jersey.Name}");

            var itemSubtotal = jersey.Price * item.Quantity;
            var patchesAmount = (item.Patches?.Count ?? 0) * PatchPriceUsd * item.Quantity;
            var totalItemPrice = itemSubtotal + patchesAmount;

            subtotal += totalItemPrice;
            totalItems += item.Quantity;
            totalWeight += jersey.Weight * item.Quantity;

            var personalitation = item.Customization is not null
                ? new[] { item.Customization.Name ?? "", item.Customization.Number ?? "" }
                : null;

            items.Add(new ItemsOrder
            {
                Quantity = item.Quantity,
                Size = [item.Size],
                Personalitation = personalitation,
                IdJersey = item.JerseyId
            });

            cartItems.Add(new CartItemDto(
                JerseyId: item.JerseyId,
                Name: jersey.Name,
                Quantity: item.Quantity,
                Price: jersey.Price,
                WeightKg: jersey.Weight,
                Size: item.Size,
                Patches: item.Patches?
                    .Select(p => new ItemPatchDto(p.Name, p.Price))
                    .ToList(),
                Customization: item.Customization,
                ImageUrl: jersey.ImageUrl
            ));
        }

        var totalPrice = subtotal + command.ShippingCost;

        var paymentStatus = await paymentService.ProcessPaymentAsync(
            command.Payment.Method, command.Payment.AccountId, totalPrice); // esto debe ser otro flujo cuando se implemente la pasarela de pagos, por ahora se asume que el pago es instantáneo y siempre exitoso

        var now = DateTime.UtcNow;

        var order = new Order
        {
            TotalPrice = totalPrice,
            TotalItems = totalItems,
            TotalWeight = totalWeight,
            Email = email,
            IdUser = userId,
            AddressId = command.IdAddress,
            ShippingCost = command.ShippingCost,
            Status = OrderStatus.CREATED.ToString(),
            PaymentMethod = command.Payment.Method,
            PaymentStatus = paymentStatus,
            PaymentAccountId = command.Payment.AccountId,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var item in items)
        {
            order.ItemsOrders.Add(item);
        }

        var created = await orderRepository.CreateAsync(order);

        logger.LogInformation("Order {OrderId} created for user {UserId}", created.IdOrder, userId);

        return new OrderConfirmationDto(
            OrderNumber: created.IdOrder.ToString(),
            Date: created.CreatedAt.ToString("o"),
            Items: cartItems,
            TotalItems: created.TotalItems,
            TotalPrice: created.TotalPrice,
            ShippingCost: created.ShippingCost,
            PaymentMethod: created.PaymentMethod,
            PaymentStatus: created.PaymentStatus,
            Status: created.Status
        );
    }
}
