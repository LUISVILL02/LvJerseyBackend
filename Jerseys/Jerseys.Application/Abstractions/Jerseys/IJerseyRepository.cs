
using Jerseys.Application.Queries.JerseysHome;
using Jerseys.Domain.Entities;

namespace Jerseys.Application.Abstractions.Jerseys;

public interface IJerseyRepository
{
    Task<List<LeagueWithJerseysResponse>> GetJerseysByCategoryAsync(int? idUser);

    /// <summary>
    /// Crea un nuevo jersey en la base de datos.
    /// </summary>
    /// <param name="jersey">Entidad Jersey a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>El jersey creado con su ID generado</returns>
    Task<Jersey> CreateAsync(Jersey jersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un jersey por su ID.
    /// </summary>
    Task<Jersey?> GetByIdAsync(int idJersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si existe un club con el ID especificado.
    /// </summary>
    Task<bool> ClubExistsAsync(int idClub, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el nombre de un club por su ID.
    /// </summary>
    Task<string?> GetClubNameAsync(int idClub, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea las relaciones entre un jersey y sus tallas.
    /// </summary>
    Task CreateSizeJerseysAsync(int idJersey, IEnumerable<int> sizeIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un jersey con todos sus datos relacionados para el detalle.
    /// Incluye: Club, League, Categories, Sizes, Patches, Favorites.
    /// </summary>
    Task<Jersey?> GetDetailByIdAsync(int idJersey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un jersey es favorito de un usuario.
    /// </summary>
    Task<bool> IsFavoriteAsync(int idJersey, int idUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todas las tallas disponibles.
    /// </summary>
    Task<List<Size>> GetAllSizesAsync(CancellationToken cancellationToken = default);
}