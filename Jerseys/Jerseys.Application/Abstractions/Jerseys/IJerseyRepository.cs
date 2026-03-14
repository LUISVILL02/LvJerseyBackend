
using Jerseys.Application.Queries.JerseysHome;

namespace Jerseys.Application.Abstractions.Jerseys;

public interface IJerseyRepository
{
    Task<List<LeagueWithJerseysResponse>> GetJerseysByCategoryAsync(int? idUser);
}