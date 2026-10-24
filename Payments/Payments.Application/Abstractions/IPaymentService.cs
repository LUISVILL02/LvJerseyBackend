namespace Payments.Application.Abstractions;

public interface IPaymentService
{
    Task<string> ProcessPaymentAsync(string method, string accountId, decimal amount, CancellationToken cancellationToken = default);
}
