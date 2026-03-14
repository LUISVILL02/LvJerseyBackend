namespace Shared.Application.Abstractions;

public interface ISender
{
    Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>;
    Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query);
}