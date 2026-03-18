using System.Text.Json;
using Jerseys.Application.Commands.CreateJersey;
using Jerseys.Application.Dtos;
using Jerseys.Application.Queries.JerseyDetail;
using Jerseys.Application.Queries.JerseysHome;
using LvJerseyApi.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;

namespace LvJerseyApi.Controllers;

[ApiController]
[Route("api/v0.0.1/[controller]")]
public class JerseyController(ISender sender) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [HttpGet]
    public async Task<IActionResult> GetJerseyshome()
    {
        var result = await sender.SendQueryAsync<HomeJerseysQuery, List<LeagueWithJerseysResponse>>(new HomeJerseysQuery());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle completo de un jersey.
    /// </summary>
    /// <param name="id">ID del jersey</param>
    /// <returns>Detalle del jersey con imágenes, tallas, patches, reseñas, etc.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(JerseyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJerseyDetail(int id)
    {
        var result = await sender.SendQueryAsync<JerseyDetailQuery, JerseyDetailResponse?>(
            new JerseyDetailQuery(id));

        if (result is null)
            return NotFound(new { Error = $"Jersey con ID {id} no encontrado." });

        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo jersey con sus imágenes y patches.
    /// Las imágenes se procesan de forma asíncrona.
    /// </summary>
    /// <param name="request">Datos del jersey, imágenes y patches</param>
    /// <returns>Información del jersey creado</returns>
    /// <remarks>
    /// Para enviar patches, use:
    /// - PatchImages: Lista de archivos de imagen (uno por patch)
    /// - PatchMetadata: JSON array con la metadata de cada patch
    /// 
    /// Ejemplo de PatchMetadata:
    /// ```json
    /// [
    ///   {"name": "Champions League", "season": "24/25"},
    ///   {"name": "La Liga", "season": "24/25"}
    /// ]
    /// ```
    /// 
    /// El orden de PatchImages debe coincidir con el orden en PatchMetadata.
    /// </remarks>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateJerseyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateJersey([FromForm] CreateJerseyRequest request)
    {
        // Convertir IFormFile a ImageUploadDto para imágenes del jersey
        var images = request.Images
            .Select(f => new ImageUploadDto(
                f.FileName,
                f.ContentType,
                f.OpenReadStream()))
            .ToList();

        // Procesar patches: deserializar metadata y combinar con imágenes
        var patches = new List<PatchUploadDto>();
        
        if (request.PatchImages.Count > 0)
        {
            List<PatchMetadataItem> patchMetadataList;

            // Leer PatchMetadata directo del form para evitar que el model binder
            // fragmente el JSON string (corta en las comas del array)
            var rawMetadata = Request.Form["PatchMetadata"].ToString().Trim();
            
            if (string.IsNullOrWhiteSpace(rawMetadata))
            {
                return BadRequest(new 
                { 
                    Error = "PatchMetadata es requerido cuando se envían PatchImages.",
                    Message = "Debe enviar un JSON array con la metadata de cada patch."
                });
            }

            try
            {
                patchMetadataList = JsonSerializer.Deserialize<List<PatchMetadataItem>>(
                    rawMetadata, 
                    JsonOptions) ?? [];
            }
            catch (JsonException ex)
            {
                return BadRequest(new 
                { 
                    Error = "PatchMetadata tiene un formato JSON inválido.",
                    Details = ex.Message,
                    ExpectedFormat = "[{\"name\":\"Nombre\",\"season\":\"24/25\"}]"
                });
            }

            // Validar que la cantidad de imágenes coincida con la metadata
            if (request.PatchImages.Count != patchMetadataList.Count)
            {
                return BadRequest(new 
                { 
                    Error = "La cantidad de PatchImages debe coincidir con la cantidad de elementos en PatchMetadata.",
                    PatchImagesCount = request.PatchImages.Count,
                    PatchMetadataCount = patchMetadataList.Count
                });
            }

            // Combinar cada imagen con su metadata correspondiente
            patches = request.PatchImages
                .Zip(patchMetadataList, (image, meta) => new PatchUploadDto(
                    Name: meta.Name,
                    Season: meta.Season,
                    Image: new ImageUploadDto(
                        image.FileName,
                        image.ContentType,
                        image.OpenReadStream())))
                .ToList();
        }

        var command = new CreateJerseyCommand(
            Name: request.Name,
            IdClub: request.IdClub,
            Type: request.Type,
            Sex: request.Sex,
            SizeIds: request.SizeIds,
            Weight: request.Weight,
            Brand: request.Brand,
            Season: request.Season,
            Price: request.Price,
            Stock: request.Stock,
            Categories: request.Categories,
            Images: images,
            Patches: patches);

        var jerseyId = await sender.SendCommandAsync<CreateJerseyCommand, int>(command);

        var response = new CreateJerseyResponse
        {
            IdJersey = jerseyId,
            Name = request.Name,
            ImagesStatus = "processing",
            ImagesCount = request.Images.Count,
            PatchesCount = patches.Count,
            Message = request.Images.Count + patches.Count > 0
                ? "Jersey creado exitosamente. Las imágenes se están procesando en segundo plano."
                : "Jersey creado exitosamente."
        };

        return CreatedAtAction(nameof(GetJerseyshome), new { id = jerseyId }, response);
    }
}
