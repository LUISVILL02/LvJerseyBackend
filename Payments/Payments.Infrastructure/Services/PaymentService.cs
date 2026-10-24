using Microsoft.Extensions.Logging;
using Payments.Application.Abstractions;

namespace Payments.Infrastructure.Services;

public class PaymentService(ILogger<PaymentService> logger) : IPaymentService
{
    public Task<string> ProcessPaymentAsync(string method, string accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Payment: Processing {Method} payment of {Amount} from account {AccountId}",
            method, amount, accountId);

        // TODO: integrar mercadopago-sdk cuando se defina el flujo de cobro real.
        return Task.FromResult("CONFIRMED");
    }
}
