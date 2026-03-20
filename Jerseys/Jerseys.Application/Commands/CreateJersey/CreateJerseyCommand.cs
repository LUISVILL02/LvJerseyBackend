using Jerseys.Application.Dtos;
using Shared.Application.Abstractions;

namespace Jerseys.Application.Commands.CreateJersey;

public sealed record CreateJerseyCommand(
    string Name,
    string ClubName,
    string Type,
    string Sex,
    IReadOnlyCollection<string> SizeSymbols,
    decimal Weight,
    string Brand,
    string Season,
    decimal Price,
    int Stock,
    IReadOnlyCollection<string> Categories,
    IReadOnlyCollection<ImageUploadDto> Images,
    IReadOnlyCollection<PatchUploadDto> Patches
) : ICommand<int>;

