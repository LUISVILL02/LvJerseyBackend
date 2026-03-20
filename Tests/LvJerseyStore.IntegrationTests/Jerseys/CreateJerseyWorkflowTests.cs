using Files.Application.Abstractions;
using Files.Domain.Enums;
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

namespace LvJerseyStore.IntegrationTests.Jerseys;

public class CreateJerseyWorkflowTests : IDisposable
{
    private readonly IJerseyRepository _jerseyRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPatchRepository _patchRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IFileUploadQueue _fileUploadQueue;
    private readonly ILogger<CreateJerseyCommandHandler> _logger;
    private readonly CreateJerseyCommandHandler _handler;
    private readonly List<FileUploadMessage> _enqueuedMessages = [];
    private readonly string _tempDirectory;

    public CreateJerseyWorkflowTests()
    {
        _jerseyRepository = Substitute.For<IJerseyRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _patchRepository = Substitute.For<IPatchRepository>();
        _fileRepository = Substitute.For<IFileRepository>();
        _fileUploadQueue = Substitute.For<IFileUploadQueue>();
        _logger = Substitute.For<ILogger<CreateJerseyCommandHandler>>();
        
        _tempDirectory = Path.Combine(Path.GetTempPath(), "LvJersey_IntegrationTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _fileUploadQueue.EnqueueAsync(
            Arg.Any<FileUploadMessage>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var message = callInfo.Arg<FileUploadMessage>();
                _enqueuedMessages.Add(message);
                return ValueTask.CompletedTask;
            });

        _handler = new CreateJerseyCommandHandler(
            _jerseyRepository,
            _categoryRepository,
            _patchRepository,
            _fileRepository,
            _fileUploadQueue,
            _logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try { Directory.Delete(_tempDirectory, true); } catch { }
        }
    }

    private static CreateJerseyCommand CreateCommand(
        string clubName = "Barcelona",
        IReadOnlyCollection<ImageUploadDto>? images = null,
        IReadOnlyCollection<PatchUploadDto>? patches = null)
    {
        return new CreateJerseyCommand(
            Name: "Barcelona Away 24/25",
            ClubName: clubName,
            Type: "Player",
            Sex: "Male",
            SizeSymbols: ["S", "M", "L"],
            Weight: 0.35m,
            Brand: "Nike",
            Season: "24/25",
            Price: 99.99m,
            Stock: 50,
            Categories: ["La Liga", "Champions League"],
            Images: images ?? [],
            Patches: patches ?? []);
    }

    private void SetupDefaultMocks(int jerseyId = 1, int idClub = 1)
    {
        _jerseyRepository.GetOrCreateClubAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns((idClub, "Barcelona"));
        _jerseyRepository.GetSizeIdsBySymbolsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns([1, 2, 3]);
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>(), Arg.Any<CancellationToken>())
            .Returns(new Jersey { IdJersey = jerseyId, Name = "Test", IdClub = idClub, ClubName = "Barcelona" });
        _categoryRepository.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Category { IdCategory = 1, Name = "La Liga" });
        _categoryRepository.CreateCategoriesJerseyAsync(Arg.Any<int>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => new Files.Domain.Entities.File { IdFile = 1, Name = callInfo.Arg<Files.Domain.Entities.File>().Name });
        _fileRepository.CreateFileJerseyAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateJersey_WithMultipleImages_ShouldEnqueueAllFilesForProcessing()
    {
        // Arrange
        var images = new List<ImageUploadDto>
        {
            new("front.jpg", "image/jpeg", new MemoryStream(new byte[1024])),
            new("back.jpg", "image/jpeg", new MemoryStream(new byte[1024])),
            new("detail.png", "image/png", new MemoryStream(new byte[512]))
        };
        var command = CreateCommand(images: images);
        const int jerseyId = 100;
        SetupDefaultMocks(jerseyId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(jerseyId);
        await _fileRepository.Received(3).CreateAsync(Arg.Any<Files.Domain.Entities.File>(), Arg.Any<CancellationToken>());
        _enqueuedMessages.Should().HaveCount(3);
        _enqueuedMessages.Should().AllSatisfy(m =>
        {
            m.IdJersey.Should().Be(jerseyId);
            m.ContainerType.Should().Be("images");
        });
    }

    [Fact]
    public async Task CreateJersey_WithImagesAndPatches_ShouldSeparateByContainerType()
    {
        // Arrange
        var images = new List<ImageUploadDto>
        {
            new("main.jpg", "image/jpeg", new MemoryStream(new byte[1024]))
        };
        var patches = new List<PatchUploadDto>
        {
            new("Champions League", "24/25", new ImageUploadDto("champions.png", "image/png", new MemoryStream(new byte[256]))),
            new("La Liga", "24/25", new ImageUploadDto("laliga.png", "image/png", new MemoryStream(new byte[256])))
        };
        var command = CreateCommand(images: images, patches: patches);
        const int jerseyId = 200;
        SetupDefaultMocks(jerseyId);
        _patchRepository.CreateAsync(Arg.Any<Patch>(), Arg.Any<CancellationToken>())
            .Returns(new Patch { IdPatch = 1, NamePatch = "Test", Season = "24/25" });
        _patchRepository.CreatePatchJerseyAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _fileRepository.CreateFilePatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(jerseyId);
        await _fileRepository.Received(3).CreateAsync(Arg.Any<Files.Domain.Entities.File>(), Arg.Any<CancellationToken>());
        await _patchRepository.Received(2).CreateAsync(Arg.Any<Patch>(), Arg.Any<CancellationToken>());
        _enqueuedMessages.Should().HaveCount(3);
        
        var imageMessages = _enqueuedMessages.Where(m => m.ContainerType == "images").ToList();
        var patchMessages = _enqueuedMessages.Where(m => m.ContainerType == "patches").ToList();
        
        imageMessages.Should().HaveCount(1);
        patchMessages.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateJersey_WithImage_ShouldCreateTemporaryFile()
    {
        // Arrange
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var images = new List<ImageUploadDto>
        {
            new("test.png", "image/png", new MemoryStream(imageData))
        };
        var command = CreateCommand(images: images);
        const int jerseyId = 300;
        SetupDefaultMocks(jerseyId);
        string? capturedTempPath = null;
        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => 
            { 
                var file = callInfo.Arg<Files.Domain.Entities.File>();
                capturedTempPath = file.TempFilePath;
                return new Files.Domain.Entities.File { IdFile = 1, TempFilePath = file.TempFilePath, Name = file.Name }; 
            });

        // Act
        await _handler.HandleAsync(command);

        // Assert
        capturedTempPath.Should().NotBeNullOrEmpty();
        capturedTempPath.Should().Contain(jerseyId.ToString());
    }

    [Fact]
    public async Task CreateJersey_ShouldFollowCorrectWorkflowOrder()
    {
        // Arrange
        var callOrder = new List<string>();
        var images = new List<ImageUploadDto>
        {
            new("test.jpg", "image/jpeg", new MemoryStream(new byte[100]))
        };
        var command = CreateCommand(images: images);

        _jerseyRepository.GetOrCreateClubAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("GetOrCreateClubAsync"); return (1, "Barcelona"); });
        _jerseyRepository.GetSizeIdsBySymbolsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("GetSizeIdsBySymbolsAsync"); return [1, 2, 3]; });
        _categoryRepository.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Category { IdCategory = 1, Name = "La Liga" });
        _categoryRepository.CreateCategoriesJerseyAsync(Arg.Any<int>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("CreateCategoriesJerseyAsync"); return Task.CompletedTask; });
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("CreateJerseyAsync"); return new Jersey { IdJersey = 1, Name = command.Name, IdClub = 1, ClubName = "Barcelona" }; });
        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("CreateFileAsync"); return new Files.Domain.Entities.File { IdFile = 1 }; });
        _fileRepository.CreateFileJerseyAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("CreateFileJerseyAsync"); return Task.CompletedTask; });
        _fileUploadQueue.EnqueueAsync(Arg.Any<FileUploadMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { callOrder.Add("EnqueueAsync"); return ValueTask.CompletedTask; });

        // Act
        await _handler.HandleAsync(command);

        // Assert
        callOrder.Should().ContainInOrder(
            "GetOrCreateClubAsync",
            "GetSizeIdsBySymbolsAsync",
            "CreateJerseyAsync",
            "CreateCategoriesJerseyAsync",
            "CreateFileAsync",
            "EnqueueAsync",
            "CreateFileJerseyAsync");
    }

    [Fact]
    public async Task CreateJersey_WhenFileCreationFails_ShouldPropagateException()
    {
        // Arrange
        var images = new List<ImageUploadDto>
        {
            new("test.jpg", "image/jpeg", new MemoryStream(new byte[100]))
        };
        var command = CreateCommand(images: images);

        _jerseyRepository.GetOrCreateClubAsync(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns((1, "Barcelona"));
        _jerseyRepository.GetSizeIdsBySymbolsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns([1, 2, 3]);
        _categoryRepository.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Category { IdCategory = 1, Name = "La Liga" });
        _categoryRepository.CreateCategoriesJerseyAsync(Arg.Any<int>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>(), Arg.Any<CancellationToken>())
            .Returns(new Jersey { IdJersey = 1, Name = command.Name, IdClub = 1, ClubName = "Barcelona" });
        
        _fileRepository.CreateAsync(Arg.Do<Files.Domain.Entities.File>(f => 
        {
            if (f.Name == "test.jpg")
                throw new InvalidOperationException("Database connection error");
        }), Arg.Any<CancellationToken>())
            .Returns(new Files.Domain.Entities.File { IdFile = 1, Name = "test.jpg" });

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection error");
    }
}
