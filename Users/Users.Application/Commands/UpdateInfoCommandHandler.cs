using Shared.Infrastructure.Abstractions;

namespace Users.Application.Commands;

public class UpdateInfoCommandHandler : ICommandHandler<UpdateInfoCommand>
{
    public Task HandleAsync(UpdateInfoCommand command)
    {
        Console.WriteLine("Creando mi primer Command con el command: " + command.UserId);
        return Task.CompletedTask;
    }
}