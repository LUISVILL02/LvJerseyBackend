using System.Linq;
using Microsoft.EntityFrameworkCore;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Queries.JerseysHome;
using Jerseys.Application.Dtos;
using Jerseys.Domain.Entities;
using Shared.Infrastructure.Data;

namespace Jerseys.Infrastructure.Services;

public class GetJerseysByCategory(ApplicationDbContext context) : IJerseyRepository
{
    public async Task<List<LeagueWithJerseysResponse>> GetJerseysByCategoryAsync(int? idUser)
    {
        if (idUser is null)
        {
            Console.WriteLine("Usuario no autenticado");
        }
        else
        {
            Console.WriteLine("Usuario autenticado");
        }
        // Cargar jerseys con sus relaciones necesarias
        var jerseysEntities = await context.Set<Jersey>()
            .Include(j => j.Club)
                .ThenInclude(c => c.League)
            .AsNoTracking()
            .ToListAsync();

        // Agrupar por liga usando una clave compuesta (Id, Name, Country)
        var grouped = jerseysEntities
            .GroupBy(j => new { j.Club.League.IdLeague, j.Club.League.Name, j.Club.League.Country })
            .ToList();

        //get favorites
        var favorites = await context.Set<Favorite>()
            .Where(f => f.IdUser == idUser)
            .AsNoTracking().ToListAsync();
        
        var result = grouped.Select(g => new LeagueWithJerseysResponse
        {
            League = g.Key.Name,
            Country = g.Key.Country,
            Jerseys = g.Select(j => new JerseyCardDto
            {
                Id = j.IdJersey,
                ImageUrl = string.Empty, // completar si tienes URL en otra entidad
                Name = j.Name,
                TypeDescription = j.Type,
                Price = j.Price ?? 0m,
                Rating = 0m,
                IsFavorite = favorites.Any(f => f.idJersey == j.IdJersey),
            }).ToList()
        }).ToList();

        return result;
    }
}