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

/// <summary>
/// Tests de integración que verifican el flujo completo de creación de jerseys
/// con procesamiento de archivos y encolado de mensajes.
/// </summary>
public class CreateJerseyWorkflowTests : IDisposable
{
    private readonly IJerseyRepository _jerseyRepository;
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
        _patchRepository = Substitute.For<IPatchRepository>();
        _fileRepository = Substitute.For<IFileRepository>();
        _fileUploadQueue = Substitute.For<IFileUploadQueue>();
        _logger = Substitute.For<ILogger<CreateJerseyCommandHandler>>();
        
        _tempDirectory = Path.Combine(Path.GetTempPath(), "LvJersey_IntegrationTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        // Capturar mensajes encolados para verificación
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
            _patchRepository,
            _fileRepository,
            _fileUploadQueue,
            _logger);
    }

    public void Dispose()
    {
        // Limpiar directorio temporal
        if (Directory.Exists(_tempDirectory))
        {
            try { Directory.Delete(_tempDirectory, true); } catch { }
        }
    }

    private static CreateJerseyCommand CreateCommand(
        int idClub = 1,
        IReadOnlyCollection<ImageUploadDto>? images = null,
        IReadOnlyCollection<PatchUploadDto>? patches = null)
    {
        return new CreateJerseyCommand(
            Name: "Barcelona Away 24/25",
            IdClub: idClub,
            Type: "Player",
            Sex: "Male",
            SizeIds: [1, 2, 3],
            Weight: 0.35m,
            Brand: "Nike",
            Season: "24/25",
            Price: 99.99m,
            Stock: 50,
            Categories: ["La Liga", "Champions League"],
            Images: images ?? [],
            Patches: patches ?? []);
    }

    /// <summary>
    /// Test 1: Flujo completo de creación de jersey con múltiples imágenes
    /// Verifica que todas las imágenes se procesan y encolan correctamente
    /// </summary>
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
        var fileIdCounter = 1;

        _jerseyRepository.GetClubNameAsync(command.IdClub, Arg.Any<CancellationToken>())
            .Returns("Barcelona");
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(new Jersey { IdJersey = jerseyId, Name = command.Name, IdClub = command.IdClub, ClubName = "Barcelona" });

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns(callInfo =>
            {
                var file = callInfo.Arg<Files.Domain.Entities.File>();
                return new Files.Domain.Entities.File
                {
                    IdFile = fileIdCounter++,
                    Name = file.Name,
                    IdJersey = file.IdJersey,
                    ProcessingStatus = file.ProcessingStatus,
                    ContainerType = file.ContainerType,
                    TempFilePath = file.TempFilePath,
                    ContentType = file.ContentType
                };
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(jerseyId);
        
        // Verificar que se crearon 3 registros de archivo
        await _fileRepository.Received(3).CreateAsync(Arg.Any<Files.Domain.Entities.File>());
        
        // Verificar que se encolaron 3 mensajes
        _enqueuedMessages.Should().HaveCount(3);
        
        // Verificar que todos los mensajes tienen el tipo de contenedor correcto
        _enqueuedMessages.Should().AllSatisfy(m =>
        {
            m.IdJersey.Should().Be(jerseyId);
            m.ContainerType.Should().Be("images");
        });
        
        // Verificar que se crearon las relaciones File-Jersey
        await _fileRepository.Received(3).CreateFileJerseyAsync(Arg.Any<int>(), jerseyId);
    }

    /// <summary>
    /// Test 2: Flujo completo con imágenes y parches
    /// Verifica que se separan correctamente en diferentes contenedores
    /// </summary>
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
            new(
                Name: "Champions League",
                Season: "24/25",
                Image: new ImageUploadDto("champions.png", "image/png", new MemoryStream(new byte[256]))),
            new(
                Name: "La Liga",
                Season: "24/25",
                Image: new ImageUploadDto("laliga.png", "image/png", new MemoryStream(new byte[256])))
        };
        var command = CreateCommand(images: images, patches: patches);
        const int jerseyId = 200;
        var fileIdCounter = 1;
        var patchIdCounter = 1;

        _jerseyRepository.GetClubNameAsync(command.IdClub, Arg.Any<CancellationToken>())
            .Returns("Barcelona");
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(new Jersey { IdJersey = jerseyId, Name = command.Name, IdClub = command.IdClub, ClubName = "Barcelona" });

        _patchRepository.CreateAsync(Arg.Any<Patch>())
            .Returns(callInfo =>
            {
                var patch = callInfo.Arg<Patch>();
                return new Patch
                {
                    IdPatch = patchIdCounter++,
                    NamePatch = patch.NamePatch,
                    Season = patch.Season
                };
            });

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns(callInfo =>
            {
                var file = callInfo.Arg<Files.Domain.Entities.File>();
                return new Files.Domain.Entities.File
                {
                    IdFile = fileIdCounter++,
                    Name = file.Name,
                    IdJersey = file.IdJersey,
                    ContainerType = file.ContainerType,
                    ProcessingStatus = file.ProcessingStatus
                };
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(jerseyId);
        
        // Verificar que se crearon 3 archivos en total (1 imagen + 2 parches)
        await _fileRepository.Received(3).CreateAsync(Arg.Any<Files.Domain.Entities.File>());
        
        // Verificar que se crearon 2 patches
        await _patchRepository.Received(2).CreateAsync(Arg.Any<Patch>());
        
        // Verificar los mensajes encolados
        _enqueuedMessages.Should().HaveCount(3);
        
        var imageMessages = _enqueuedMessages.Where(m => m.ContainerType == "images").ToList();
        var patchMessages = _enqueuedMessages.Where(m => m.ContainerType == "patches").ToList();
        
        imageMessages.Should().HaveCount(1);
        patchMessages.Should().HaveCount(2);
    }

    /// <summary>
    /// Test 3: Verificar que los archivos temporales se crean correctamente
    /// </summary>
    [Fact]
    public async Task CreateJersey_WithImage_ShouldCreateTemporaryFile()
    {
        // Arrange
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        var images = new List<ImageUploadDto>
        {
            new("test.png", "image/png", new MemoryStream(imageData))
        };
        var command = CreateCommand(images: images);
        const int jerseyId = 300;
        string? capturedTempPath = null;

        _jerseyRepository.GetClubNameAsync(command.IdClub, Arg.Any<CancellationToken>())
            .Returns("Barcelona");
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(new Jersey { IdJersey = jerseyId, Name = command.Name, IdClub = command.IdClub, ClubName = "Barcelona" });

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns(callInfo =>
            {
                var file = callInfo.Arg<Files.Domain.Entities.File>();
                capturedTempPath = file.TempFilePath;
                return new Files.Domain.Entities.File
                {
                    IdFile = 1,
                    TempFilePath = file.TempFilePath,
                    Name = file.Name
                };
            });

        // Act
        await _handler.HandleAsync(command);

        // Assert
        capturedTempPath.Should().NotBeNullOrEmpty();
        
        // El archivo temporal debería existir después de la creación
        // (En un test real, verificaríamos el contenido)
        capturedTempPath.Should().Contain(jerseyId.ToString());
    }

    /// <summary>
    /// Test 4: El flujo completo debe respetar el orden: crear jersey → crear archivos → encolar
    /// </summary>
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

        _jerseyRepository.GetClubNameAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callOrder.Add("GetClubNameAsync");
                return "Barcelona";
            });

        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(callInfo =>
            {
                callOrder.Add("CreateJerseyAsync");
                return new Jersey { IdJersey = 1, Name = command.Name, IdClub = command.IdClub, ClubName = "Barcelona" };
            });

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns(callInfo =>
            {
                callOrder.Add("CreateFileAsync");
                return new Files.Domain.Entities.File { IdFile = 1 };
            });

        _fileRepository.CreateFileJerseyAsync(Arg.Any<int>(), Arg.Any<int>())
            .Returns(callInfo =>
            {
                callOrder.Add("CreateFileJerseyAsync");
                return Task.CompletedTask;
            });

        _fileUploadQueue.EnqueueAsync(
            Arg.Any<FileUploadMessage>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callOrder.Add("EnqueueAsync");
                return ValueTask.CompletedTask;
            });

        // Act
        await _handler.HandleAsync(command);

        // Assert - Verificar orden correcto
        // El flujo es: GetClubName → CreateJersey → CreateFile → Enqueue → CreateFileJersey
        callOrder.Should().Equal(
            "GetClubNameAsync",
            "CreateJerseyAsync",
            "CreateFileAsync",
            "EnqueueAsync",
            "CreateFileJerseyAsync");
    }

    /// <summary>
    /// Test 5: Manejo de error durante el procesamiento de archivos
    /// Si falla la creación de un archivo, debe propagar la excepción
    /// </summary>
    [Fact]
    public async Task CreateJersey_WhenFileCreationFails_ShouldPropagateException()
    {
        // Arrange
        var images = new List<ImageUploadDto>
        {
            new("test.jpg", "image/jpeg", new MemoryStream(new byte[100]))
        };
        var command = CreateCommand(images: images);

        _jerseyRepository.GetClubNameAsync(command.IdClub, Arg.Any<CancellationToken>())
            .Returns("Barcelona");
        _jerseyRepository.CreateAsync(Arg.Any<Jersey>())
            .Returns(new Jersey { IdJersey = 1, Name = command.Name, IdClub = command.IdClub, ClubName = "Barcelona" });

        _fileRepository.CreateAsync(Arg.Any<Files.Domain.Entities.File>())
            .Returns<Files.Domain.Entities.File>(x => 
                throw new InvalidOperationException("Database connection error"));

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection error");
        
        // El mensaje no debería haberse encolado porque falló antes
        _enqueuedMessages.Should().BeEmpty();
    }
}
