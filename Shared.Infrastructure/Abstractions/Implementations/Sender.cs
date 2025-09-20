using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infrastructure.Abstractions.Implementations;

public class Sender(IServiceProvider provider) : ISender
{
    public async Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command)
    {
        var validators = provider.GetServices<IValidator<TCommand>>();
        foreach (var v in validators)
        {
            var result = await v.ValidateAsync(command);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }
        var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.HandleAsync(command);
        
    }

    public async Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query)
    {
        var validators = provider.GetServices<IValidator<TQuery>>();
        foreach (var v in validators)
        {
            var result = await v.ValidateAsync(query);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }
        
        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.HandleAsync(query);
    }
}