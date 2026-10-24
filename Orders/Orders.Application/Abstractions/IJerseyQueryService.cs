namespace Orders.Application.Abstractions;


public interface IJerseyQueryService
{
    Task<JerseyInfoDto?> GetByIdAsync(int idJersey, CancellationToken cancellationToken = default);
}
