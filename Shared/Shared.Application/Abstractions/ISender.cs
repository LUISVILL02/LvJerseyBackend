namespace Shared.Application.Abstractions;

public interface ISender
{
    Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command);
    Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query);
}