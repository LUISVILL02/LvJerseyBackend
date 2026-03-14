using Jerseys.Domain.Entities;

namespace Jerseys.Application.Abstractions.Patches;

public interface IPatchRepository
{
    /// <summary>
    /// Crea un nuevo parche en la base de datos.
    /// </summary>
    /// <param name="patch">Entidad Patch a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El parche creado con su ID generado</returns>
    Task<Patch> CreateAsync(Patch patch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la relación entre un parche y un jersey.
    /// </summary>
    /// <param name="idPatch">ID del parche</param>
    /// <param name="idJersey">ID del jersey</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task CreatePatchJerseyAsync(int idPatch, int idJersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un parche por su ID.
    /// </summary>
    Task<Patch?> GetByIdAsync(int idPatch, CancellationToken cancellationToken = default);
}
