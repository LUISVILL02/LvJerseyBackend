namespace Shared.Infrastructure.Abstractions;

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command);
}