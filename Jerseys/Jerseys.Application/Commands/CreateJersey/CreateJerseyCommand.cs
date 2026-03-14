using Jerseys.Application.Dtos;
using Shared.Application.Abstractions;

namespace Jerseys.Application.Commands.CreateJersey;

public sealed record CreateJerseyCommand(
    string Name,
    int IdClub,
    string Type,
    string Sex,
    string Size,
    decimal Weight,
    string Brand,
    string Season,
    decimal Price,
    int Stock,
    IReadOnlyCollection<string> Categories,
    IReadOnlyCollection<ImageUploadDto> Images,
    IReadOnlyCollection<PatchUploadDto> Patches
) : ICommand<int>;

