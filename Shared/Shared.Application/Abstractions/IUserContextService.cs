namespace Shared.Application.Abstractions;

public interface IUserContextService
{
    int? GetUserId(); // Devuelve el ID del usuario autenticado o null si no está autenticado
}
