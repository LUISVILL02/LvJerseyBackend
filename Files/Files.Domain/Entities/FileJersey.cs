namespace Files.Domain.Entities;

public class FileJersey
{
    public int IdFile { get; set; }
    public File File { get; set; } = null!;

    public int IdJersey { get; set; }
}
