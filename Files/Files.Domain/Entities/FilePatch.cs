namespace Files.Domain.Entities;

public class FilePatch
{
    public int IdFile { get; set; }
    public File File { get; set; } = null!;

    public int IdPatch { get; set; }
}
