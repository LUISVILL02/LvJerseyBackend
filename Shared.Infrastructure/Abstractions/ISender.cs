namespace Shared.Infrastructure.Abstractions;

public interface ISender
{
    Task SendAsync<TCommand>(TCommand command);
    Task<TResult> SendAsync<TQuery, TResult>(TQuery query);
}