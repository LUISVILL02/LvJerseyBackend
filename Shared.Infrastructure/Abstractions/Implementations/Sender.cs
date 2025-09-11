using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Abstractions.Implementations;

public class Sender(IServiceProvider provider) : ISender
{
    public async Task SendAsync<TCommand>(TCommand command)
    {
        var handler = provider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.HandleAsync(command);
    }

    public async Task<TResult> SendAsync<TQuery, TResult>(TQuery query)
    {
        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.HandleAsync(query);
    }
}