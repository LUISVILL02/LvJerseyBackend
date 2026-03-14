namespace LvJerseyApi.Request;

public sealed class CreateJerseyRequest
{
    public string Name { get; set; } = default!;
    public string Club { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public string Type { get; set; } = default!;
    public string Sex { get; set; } = default!;
    public string Size { get; set; } = default!;

    public decimal Weight { get; set; }
    public string Brand { get; set; } = default!;
    public string Season { get; set; } = default!;

    public List<string> Categories { get; set; } = new();
    public List<IFormFile> Images { get; set; } = new();
    public List<IFormFile> Patches { get; set; } = new();
}