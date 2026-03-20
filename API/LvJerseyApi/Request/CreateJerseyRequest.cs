using System.Text.Json.Serialization;

namespace LvJerseyApi.Request;

/// <summary>
/// Request para crear un nuevo jersey con sus imágenes y patches.
/// </summary>
public sealed class CreateJerseyRequest
{
    /// <summary>
    /// Nombre del jersey (ej: "Real Madrid Home 24/25")
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Nombre del club al que pertenece el jersey.
    /// </summary>
    public string ClubName { get; set; } = default!;

    /// <summary>
    /// Precio del jersey.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Cantidad en stock.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Tipo de jersey: Player, Fan, Retro
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Sexo: Male, Female, Unisex
    /// </summary>
    public string Sex { get; set; } = default!;

    /// <summary>
    /// Símbolos de las tallas disponibles para este jersey (ej: S, M, L, XL).
    /// </summary>
    public List<string> SizeSymbols { get; set; } = new();

    /// <summary>
    /// Peso en kilogramos.
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Marca del jersey (ej: Nike, Adidas, Puma)
    /// </summary>
    public string Brand { get; set; } = default!;

    /// <summary>
    /// Temporada (formato: XX/XX, ej: 24/25)
    /// </summary>
    public string Season { get; set; } = default!;

    /// <summary>
    /// Lista de nombres de categorías.
    /// </summary>
    public List<string> Categories { get; set; } = new();

    /// <summary>
    /// Imágenes principales del jersey (máximo 10).
    /// </summary>
    public List<IFormFile> Images { get; set; } = new();

    /// <summary>
    /// Imágenes de los patches/badges del jersey (máximo 5).
    /// El orden debe coincidir con el array en PatchMetadata.
    /// </summary>
    public List<IFormFile> PatchImages { get; set; } = new();

    /// <summary>
    /// Metadata de los patches en formato JSON string.
    /// Debe ser un array JSON donde cada elemento tiene "name" y "season".
    /// El orden debe coincidir con PatchImages.
    /// Ejemplo: [{"name":"Champions League","season":"24/25"},{"name":"La Liga","season":"24/25"}]
    /// </summary>
    public string? PatchMetadata { get; set; }
}

/// <summary>
/// Representa la metadata de un patch individual (para deserialización del JSON).
/// </summary>
public sealed class PatchMetadataItem
{
    /// <summary>
    /// Nombre del patch (ej: "Champions League").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Temporada en la que aplica el patch (ej: "24/25").
    /// </summary>
    [JsonPropertyName("season")]
    public string Season { get; set; } = default!;
}
