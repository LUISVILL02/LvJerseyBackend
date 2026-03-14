namespace Files.Domain.Entities;

public sealed class File
{
    public int IdFile { get; set; }

    public string Url { get; set; } = null!;

    public string? Name { get; set; }

    public string[]? Format { get; set; }

    public int? IdJersey { get; set; }

    public int? IdUser { get; set; }
    
    public ICollection<FileJersey> FileJerseys { get; set; } = new List<FileJersey>();
    public ICollection<FilePatch> FilePatches { get; set; } = new List<FilePatch>();
}
