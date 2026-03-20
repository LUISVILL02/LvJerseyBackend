using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;

namespace Jerseys.Infrastructure.Services;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<Category> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

        if (existing != null)
            return existing;

        var newCategory = new Category { Name = name };
        context.Set<Category>().Add(newCategory);
        await context.SaveChangesAsync(cancellationToken);

        return newCategory;
    }

    public async Task CreateCategoriesJerseyAsync(
        int idJersey,
        IEnumerable<int> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var categoriesJersey = categoryIds.Select(idCategory => new CategoriesJersey
        {
            IdCategory = idCategory,
            IdJersey = idJersey
        });

        context.Set<CategoriesJersey>().AddRange(categoriesJersey);
        await context.SaveChangesAsync(cancellationToken);
    }
}
