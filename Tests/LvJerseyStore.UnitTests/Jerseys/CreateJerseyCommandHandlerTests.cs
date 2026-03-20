using Files.Application.Abstractions;
using FluentAssertions;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Abstractions.Patches;
using Jerseys.Application.Commands.CreateJersey;
using Jerseys.Application.Dtos;
using Jerseys.Domain.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Application.Abstractions;
using Shared.Application.Dtos;

namespace LvJerseyStore.UnitTests.Jerseys;

public class CreateJerseyCommandHandlerTests
{
    private readonly IJerseyRepository _jerseyRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPatchRepository _patchRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IFileUploadQueue _fileUploadQueue;
    private readonly ILogger<CreateJerseyCommandHandler> _logger;
    private readonly CreateJerseyCommandHandler _handler;

    public CreateJerseyCommandHandlerTests()
    {
        _jerseyRepository = Substitute.For<IJerseyRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _patchRepository = Substitute.For<IPatchRepository>();
        _fileRepository = Substitute.For<IFileRepository>();
        _fileUploadQueue = Substitute.For<IFileUploadQueue>();
        _logger = Substitute.For<ILogger<CreateJerseyCommandHandler>>();

        _handler = new CreateJerseyCommandHandler(
            _jerseyRepository,
            _categoryRepository,
            _patchRepository,
            _fileRepository,
            _fileUploadQueue,
            _logger);
    }

    private static CreateJerseyCommand CreateValidCommand(
        IReadOnlyCollection<ImageUploadDto>? images = null,
        IReadOnlyCollection<PatchUploadDto>? patches = null)
    {
        return new CreateJerseyCommand(
            Name: "Real Madrid Home 24/25",
            ClubName: "Real Madrid",
            Type: "Player",
            Sex: "Male",
            SizeSymbols: ["S", "M", "L"],
            Weight: 0.3m,
            Brand: "Adidas",
            Season: "24/25",
            Price: 89.99m,
            Stock: 100,
            Categories: ["Primera División", "Champions League"],
            Images: images ?? [],
            Patches: patches ?? []);
    }

    private static Jersey CreateJerseyWithId(int id, CreateJerseyCommand command, int idClub)
    {
        return new Jersey
        {
            IdJersey = id,
            Name = command.Name,
            IdClub = idClub,
            ClubName = "Real Madrid",
            Type = command.Type,
            Sex = command.Sex,
            Weight = command.Weight,
            Brand = command.Brand,
            Season = command.Season,
            Price = command.Price,
            Stock = command.Stock
        };
    }

    /// <summary>
    /// Test 1: Verifica que se crea un jersey correctamente cuando el club existe y no hay imágenes
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenClubExistsAndNoImages_ShouldCreateJerseyAndReturnId()
    {
        // Arrange
        var command = CreateValidCommand();
        const int expectedJerseyId = 42;
        const int idClub = 1;

        _jerseyRepository.GetOrCreateClubAsync(command.ClubName, command.Categories, Arg.Any<CancellationToken>())
            .Returns((idClub, "Real Madrid"));
        _jerseyRepository.GetSizeIdsBySymbolsAsync(command.SizeSymbols, Arg.Any<CancellationToken>())
            .Returns([1, 2, 3]);
        _categoryRepository.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Category { IdCategory = 1, Name = "Primera División" });
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(callInfo => CreateJerseyWithId(expectedJerseyId, command, idClub));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedJerseyId);
        
        await _jerseyRepository.Received(1).CreateAsync(Arg.Is<Jersey>(j =>
            j.Name == command.Name &&
            j.IdClub == idClub &&
            j.Type == command.Type &&
            j.Price == command.Price &&
            j.Stock == command.Stock));
    }

    /// <summary>
    /// Test 2: Verifica que se lanza excepción cuando el club no existe y no hay liga en categorías
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenClubDoesNotExistAndNoLeagueInCategories_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = CreateValidCommand();

        _jerseyRepository.GetOrCreateClubAsync(command.ClubName, command.Categories, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<(int, string)>(new InvalidOperationException($"No se encontró una liga válida en las categorías. El club '{command.ClubName}' no puede crearse sin liga.")));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"No se encontró una liga válida en las categorías. El club '{command.ClubName}' no puede crearse sin liga.");
        
        await _jerseyRepository.DidNotReceive().CreateAsync(Arg.Any<Jersey>());
    }

    /// <summary>
    /// Test 3: Verifica que las imágenes se procesan y encolan correctamente
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithImages_ShouldCreateFileRecordsAndEnqueueMessages()
    {
        // Arrange
        var imageContent = new MemoryStream([1, 2, 3, 4, 5]);
        var images = new List<ImageUploadDto>
        {
            new("test-image.jpg", "image/jpeg", imageContent)
        };
        var command = CreateValidCommand(images: images);
        const int expectedJerseyId = 42;
        const int expectedFileId = 100;
        const int idClub = 1;

        _jerseyRepository.GetOrCreateClubAsync(command.ClubName, command.Categories, Arg.Any<CancellationToken>())
            .Returns((idClub, "Real Madrid"));
        _jerseyRepository.GetSizeIdsBySymbolsAsync(command.SizeSymbols, Arg.Any<CancellationToken>())
            .Returns([1, 2, 3]);
        _categoryRepository.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Category { IdCategory = 1, Name = "Primera División" });
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(callInfo => CreateJerseyWithId(expectedJerseyId, command, idClub));

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns(callInfo =>
            {
                var inputFile = callInfo.Arg<Files.Domain.Entities.File>();
                return new Files.Domain.Entities.File
                {
                    IdFile = expectedFileId,
                    Name = inputFile.Name,
                    Url = inputFile.Url,
                    IdJersey = inputFile.IdJersey,
                    ProcessingStatus = inputFile.ProcessingStatus,
                    ContainerType = inputFile.ContainerType,
                    TempFilePath = inputFile.TempFilePath,
                    ContentType = inputFile.ContentType,
                    CreatedAt = inputFile.CreatedAt
                };
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedJerseyId);
        
        // Verificar que se creó el registro de archivo
        await _fileRepository.Received(1).CreateAsync(Arg.Is<Files.Domain.Entities.File>(f =>
            f.Name == "test-image.jpg" &&
            f.IdJersey == expectedJerseyId &&
            f.ContainerType == "images" &&
            f.ProcessingStatus == Files.Domain.Enums.FileProcessingStatus.Pending));
        
        // Verificar que se creó la relación File-Jersey
        await _fileRepository.Received(1).CreateFileJerseyAsync(expectedFileId, expectedJerseyId);
        
        // Verificar que se encoló el mensaje
        await _fileUploadQueue.Received(1).EnqueueAsync(
            Arg.Is<FileUploadMessage>(m =>
                m.IdFile == expectedFileId &&
                m.IdJersey == expectedJerseyId &&
                m.ContainerType == "images"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Test 4: Verifica que los patches se crean con su metadata y relaciones
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithPatches_ShouldCreatePatchEntitiesAndRelationships()
    {
        // Arrange
        var patchImage = new ImageUploadDto("champions.png", "image/png", new MemoryStream([1, 2, 3]));
        var patches = new List<PatchUploadDto>
        {
            new(Name: "Champions League", Season: "24/25", Image: patchImage)
        };
        var command = CreateValidCommand(patches: patches);
        const int expectedJerseyId = 42;
        const int expectedPatchId = 10;
        const int expectedFileId = 100;
        const int idClub = 1;

        _jerseyRepository.GetOrCreateClubAsync(command.ClubName, command.Categories, Arg.Any<CancellationToken>())
            .Returns((idClub, "Real Madrid"));
        _jerseyRepository.GetSizeIdsBySymbolsAsync(command.SizeSymbols, Arg.Any<CancellationToken>())
            .Returns([1, 2, 3]);
        _categoryRepository.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Category { IdCategory = 1, Name = "Primera División" });
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(callInfo => CreateJerseyWithId(expectedJerseyId, command, idClub));

        _patchRepository.CreateAsync(Arg.Any<Patch>())
            .Returns(new Patch { IdPatch = expectedPatchId, NamePatch = "Champions League", Season = "24/25" });

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns(new Files.Domain.Entities.File { IdFile = expectedFileId });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedJerseyId);

        // Verificar que se creó el patch
        await _patchRepository.Received(1).CreateAsync(Arg.Is<Patch>(p =>
            p.NamePatch.Contains("Champions League") &&
            p.Season.Contains("24/25")));

        // Verificar que se creó la relación Patch-Jersey
        await _patchRepository.Received(1).CreatePatchJerseyAsync(expectedPatchId, expectedJerseyId);

        // Verificar que se creó la relación File-Patch
        await _fileRepository.Received(1).CreateFilePatchAsync(expectedFileId, expectedPatchId);
    }
}
