using Files.Application.Abstractions;
using Files.Domain.Entities;
using Files.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using File = Files.Domain.Entities.File;

namespace Files.Infrastructure.Services;

public class FileRepository(ApplicationDbContext context) : IFileRepository
{
    public async Task<File> CreateAsync(File file, CancellationToken cancellationToken = default)
    {
        context.Set<File>().Add(file);
        await context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task<File?> GetByIdAsync(int idFile, CancellationToken cancellationToken = default)
    {
        return await context.Set<File>()
            .FirstOrDefaultAsync(f => f.IdFile == idFile, cancellationToken);
    }

    public async Task<File> UpdateAsync(File file, CancellationToken cancellationToken = default)
    {
        file.UpdatedAt = DateTime.UtcNow;
        context.Set<File>().Update(file);
        await context.SaveChangesAsync(cancellationToken);
        return file;
    }

    public async Task UpdateProcessingStatusAsync(
        int idFile,
        FileProcessingStatus status,
        string? url = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var file = await context.Set<File>()
            .FirstOrDefaultAsync(f => f.IdFile == idFile, cancellationToken);

        if (file is null)
            return;

        file.ProcessingStatus = status;
        file.UpdatedAt = DateTime.UtcNow;

        if (url is not null)
            file.Url = url;

        if (errorMessage is not null)
            file.ErrorMessage = errorMessage;

        // Limpiar la ruta temporal si el procesamiento fue exitoso
        if (status == FileProcessingStatus.Completed)
            file.TempFilePath = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<File>> GetByJerseyIdAsync(int idJersey, CancellationToken cancellationToken = default)
    {
        return await context.Set<File>()
            .Where(f => f.IdJersey == idJersey)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<File?> GetByPatchIdAsync(int idPatch, CancellationToken cancellationToken = default)
    {
        return await context.Set<FilePatch>()
            .Where(fp => fp.IdPatch == idPatch)
            .Select(fp => fp.File)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string>> GetUrlsByPatchIdsAsync(IEnumerable<int> patchIds, CancellationToken cancellationToken = default)
    {
        var patchIdList = patchIds.ToList();
        
        var result = await context.Set<FilePatch>()
            .Where(fp => patchIdList.Contains(fp.IdPatch))
            .Select(fp => new { fp.IdPatch, fp.File.Url })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result.ToDictionary(x => x.IdPatch, x => x.Url);
    }

    public async Task DeleteAsync(int idFile, CancellationToken cancellationToken = default)
    {
        var file = await context.Set<File>()
            .FirstOrDefaultAsync(f => f.IdFile == idFile, cancellationToken);

        if (file is not null)
        {
            context.Set<File>().Remove(file);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CreateFileJerseyAsync(int idFile, int idJersey, CancellationToken cancellationToken = default)
    {
        var fileJersey = new FileJersey
        {
            IdFile = idFile,
            IdJersey = idJersey
        };

        context.Set<FileJersey>().Add(fileJersey);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateFilePatchAsync(int idFile, int idPatch, CancellationToken cancellationToken = default)
    {
        var filePatch = new FilePatch
        {
            IdFile = idFile,
            IdPatch = idPatch
        };

        context.Set<FilePatch>().Add(filePatch);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<File>> CreateManyAsync(IEnumerable<File> files, CancellationToken cancellationToken = default)
    {
        var fileList = files.ToList();
        
        context.Set<File>().AddRange(fileList);
        await context.SaveChangesAsync(cancellationToken);
        
        return fileList;
    }

    public async Task<Dictionary<int, string>> GetFirstImageUrlsByJerseyIdsAsync(
        IEnumerable<int> jerseyIds, 
        CancellationToken cancellationToken = default)
    {
        var fileJerseys = await context.Set<FileJersey>()
            .Include(fj => fj.File)
            .Where(fj => jerseyIds.Contains(fj.IdJersey))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return fileJerseys
            .GroupBy(fj => fj.IdJersey)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(fj => fj.File.CreatedAt)
                    .FirstOrDefault()?.File.Url ?? string.Empty);
    }
}
