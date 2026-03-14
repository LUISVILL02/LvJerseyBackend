using Shared.Application.Abstractions;

namespace Jerseys.Application.Commands.CreateJersey;

public class CreateJerseyCommandHandler : ICommandHandler<CreateJerseyCommand, int>
{
    public Task<int> HandleAsync(CreateJerseyCommand command)
    {
        throw new NotImplementedException();
    }
}