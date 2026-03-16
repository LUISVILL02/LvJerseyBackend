using Files.Application.Abstractions;
using Files.Domain.Enums;
using Jerseys.Application.Abstractions.Jerseys;
using Jerseys.Application.Abstractions.Patches;
using Jerseys.Application.Dtos;
using Jerseys.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions;
using Shared.Application.Dtos;
using File = Files.Domain.Entities.File;

namespace Jerseys.Application.Commands.CreateJersey;

public class CreateJerseyCommandHandler(
    IJerseyRepository jerseyRepository,
    IPatchRepository patchRepository,
    IFileRepository fileRepository,
    IFileUploadQueue fileUploadQueue,
    ILogger<CreateJerseyCommandHandler> logger) : ICommandHandler<CreateJerseyCommand, int>
{
    public async Task<int> HandleAsync(CreateJerseyCommand command)
    {
        // 1. Validar que el club existe y obtener su nombre
        var clubName = await jerseyRepository.GetClubNameAsync(command.IdClub);
        if (clubName is null)
        {
            throw new InvalidOperationException($"El club con ID {command.IdClub} no existe.");
        }

        // 2. Crear la entidad Jersey
        var jersey = new Jersey
        {
            Name = command.Name,
            IdClub = command.IdClub,
            ClubName = clubName,
            Type = command.Type,
            Sex = command.Sex,
            Weight = command.Weight,
            Brand = command.Brand,
            Season = command.Season,
            Price = command.Price,
            Stock = command.Stock
        };

        // 3. Guardar el jersey en la base de datos
        var createdJersey = await jerseyRepository.CreateAsync(jersey);
        var jerseyId = createdJersey.IdJersey;

        logger.LogInformation("Jersey creado con ID: {JerseyId}", jerseyId);

        // 4. Crear relaciones con las tallas
        if (command.SizeIds.Count > 0)
        {
            await jerseyRepository.CreateSizeJerseysAsync(jerseyId, command.SizeIds);
            logger.LogInformation("Tallas asociadas al jersey {JerseyId}: {SizeIds}", jerseyId, string.Join(", ", command.SizeIds));
        }

        // 5. Procesar imágenes del jersey
        await ProcessImagesAsync(command.Images, jerseyId);

        // 6. Procesar patches (crear entidades Patch, relaciones y archivos)
        await ProcessPatchesAsync(command.Patches, jerseyId);

        return jerseyId;
    }

    private async Task ProcessImagesAsync(
        IReadOnlyCollection<ImageUploadDto> files,
        int jerseyId)
    {
        if (files.Count == 0)
            return;

        var tempDirectory = Path.Combine(Path.GetTempPath(), "LvJersey", jerseyId.ToString());
        Directory.CreateDirectory(tempDirectory);

        foreach (var fileDto in files)
        {
            try
            {
                var createdFile = await SaveFileAndEnqueueAsync(
                    fileDto, 
                    jerseyId, 
                    tempDirectory, 
                    "images");

                // Crear la relación File-Jersey
                await fileRepository.CreateFileJerseyAsync(createdFile.IdFile, jerseyId);

                logger.LogInformation(
                    "Imagen {FileName} encolada para jersey {JerseyId}, FileId: {FileId}",
                    fileDto.FileName, jerseyId, createdFile.IdFile);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error al procesar imagen {FileName} para jersey {JerseyId}",
                    fileDto.FileName, jerseyId);
                throw;
            }
        }
    }

    private async Task ProcessPatchesAsync(
        IReadOnlyCollection<PatchUploadDto> patches,
        int jerseyId)
    {
        if (patches.Count == 0)
            return;

        var tempDirectory = Path.Combine(Path.GetTempPath(), "LvJersey", jerseyId.ToString(), "patches");
        Directory.CreateDirectory(tempDirectory);

        foreach (var patchDto in patches)
        {
            try
            {
                // 1. Crear la entidad Patch
                var patch = new Patch
                {
                    NamePatch = patchDto.Name,
                    Season = patchDto.Season
                };

                var createdPatch = await patchRepository.CreateAsync(patch);

                logger.LogInformation(
                    "Patch creado con ID: {PatchId} para jersey {JerseyId}",
                    createdPatch.IdPatch, jerseyId);

                // 2. Crear la relación Patch-Jersey
                await patchRepository.CreatePatchJerseyAsync(createdPatch.IdPatch, jerseyId);

                // 3. Procesar la imagen del patch
                var createdFile = await SaveFileAndEnqueueAsync(
                    patchDto.Image, 
                    jerseyId, 
                    tempDirectory, 
                    "patches");

                // 4. Crear la relación File-Patch
                await fileRepository.CreateFilePatchAsync(createdFile.IdFile, createdPatch.IdPatch);

                logger.LogInformation(
                    "Patch {PatchId} con imagen {FileId} encolado para jersey {JerseyId}",
                    createdPatch.IdPatch, createdFile.IdFile, jerseyId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error al procesar patch para jersey {JerseyId}",
                    jerseyId);
                throw;
            }
        }
    }

    private async Task<File> SaveFileAndEnqueueAsync(
        ImageUploadDto fileDto,
        int jerseyId,
        string tempDirectory,
        string containerType)
    {
        // Generar nombre único para el archivo temporal
        var uniqueFileName = $"{Guid.NewGuid()}_{fileDto.FileName}";
        var tempFilePath = Path.Combine(tempDirectory, uniqueFileName);

        // Guardar archivo temporalmente en disco
        await using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
        {
            await fileDto.Content.CopyToAsync(fileStream);
        }

        // Crear registro en la base de datos con estado Pending
        var fileEntity = new File
        {
            Url = string.Empty, // Se actualizará cuando se suba a Storj
            Name = fileDto.FileName,
            IdJersey = jerseyId,
            ProcessingStatus = FileProcessingStatus.Pending,
            ContainerType = containerType,
            TempFilePath = tempFilePath,
            ContentType = fileDto.ContentType,
            CreatedAt = DateTime.UtcNow
        };

        var createdFile = await fileRepository.CreateAsync(fileEntity);

        // Encolar mensaje para procesamiento asíncrono
        var queueMessage = new FileUploadMessage
        {
            IdFile = createdFile.IdFile,
            IdJersey = jerseyId,
            TempFilePath = tempFilePath,
            FileName = uniqueFileName,
            ContentType = fileDto.ContentType,
            ContainerType = containerType,
            EnqueuedAt = DateTime.UtcNow
        };

        await fileUploadQueue.EnqueueAsync(queueMessage);

        return createdFile;
    }
}
