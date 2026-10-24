using Microsoft.EntityFrameworkCore;
using Orders.Application.Abstractions;
using Orders.Domain.Entities;
using Shared.Infrastructure.Data;

namespace Orders.Infrastructure.Services;

public class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        context.Set<Order>().Add(order);
        await context.SaveChangesAsync(cancellationToken);
        return order;
    }
}
