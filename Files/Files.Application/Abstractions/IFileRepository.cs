using Files.Domain.Enums;
using File = Files.Domain.Entities.File;

namespace Files.Application.Abstractions;

public interface IFileRepository
{
    /// <summary>
    /// Crea un nuevo registro de archivo en la base de datos.
    /// </summary>
    Task<File> CreateAsync(File file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un archivo por su ID.
    /// </summary>
    Task<File?> GetByIdAsync(int idFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un archivo existente.
    /// </summary>
    Task<File> UpdateAsync(File file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza el estado de procesamiento de un archivo.
    /// </summary>
    Task UpdateProcessingStatusAsync(
        int idFile,
        FileProcessingStatus status,
        string? url = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los archivos de un jersey.
    /// </summary>
    Task<IReadOnlyList<File>> GetByJerseyIdAsync(int idJersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el primer archivo (imagen) asociado a un patch.
    /// </summary>
    Task<File?> GetByPatchIdAsync(int idPatch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los archivos de múltiples patches.
    /// </summary>
    Task<Dictionary<int, string>> GetUrlsByPatchIdsAsync(IEnumerable<int> patchIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un archivo.
    /// </summary>
    Task DeleteAsync(int idFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la relación File-Jersey en la tabla de unión.
    /// </summary>
    Task CreateFileJerseyAsync(int idFile, int idJersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la relación File-Patch en la tabla de unión.
    /// </summary>
    Task CreateFilePatchAsync(int idFile, int idPatch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea múltiples archivos en una transacción.
    /// </summary>
    Task<IReadOnlyList<File>> CreateManyAsync(IEnumerable<File> files, CancellationToken cancellationToken = default);

    Task<Dictionary<int, string>> GetFirstImageUrlsByJerseyIdsAsync(IEnumerable<int> jerseyIds, CancellationToken cancellationToken = default);
}
