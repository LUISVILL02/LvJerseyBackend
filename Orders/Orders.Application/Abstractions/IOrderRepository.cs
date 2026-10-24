using Orders.Domain.Entities;

namespace Orders.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
}
