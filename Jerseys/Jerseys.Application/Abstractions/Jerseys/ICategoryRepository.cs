using Jerseys.Domain.Entities;

namespace Jerseys.Application.Abstractions.Jerseys;

public interface ICategoryRepository
{
    Task<Category> GetOrCreateAsync(string name, CancellationToken cancellationToken = default);

    Task CreateCategoriesJerseyAsync(int idJersey, IEnumerable<int> categoryIds, CancellationToken cancellationToken = default);
}
