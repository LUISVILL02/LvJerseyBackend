namespace Jerseys.Application.Dtos;

public sealed record ImageUploadDto(
    string FileName,
    string ContentType,
    Stream Content
);