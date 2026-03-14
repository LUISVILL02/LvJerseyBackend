using Jerseys.Application.Abstractions.Jerseys;
using Shared.Application.Abstractions;
using Shared.Infrastructure.Abstractions;

namespace Jerseys.Application.Queries.JerseysHome;

public class HomeJerseysQueryHandler(IJerseyRepository repository, IUserContextService userService) : IQueryHandler<HomeJerseysQuery, List<LeagueWithJerseysResponse>>
{
    public async Task<List<LeagueWithJerseysResponse>> HandleAsync(HomeJerseysQuery query)
    {
        var userId = userService.GetUserId();
        
        return await repository.GetJerseysByCategoryAsync(userId);
    }
}