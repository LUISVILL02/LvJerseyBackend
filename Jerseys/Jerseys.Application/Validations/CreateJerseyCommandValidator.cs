using FluentValidation;
using Jerseys.Application.Commands.CreateJersey;

namespace Jerseys.Application.Validations;

public class CreateJerseyCommandValidator : AbstractValidator<CreateJerseyCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly string[] AllowedTypes = ["Player", "Fan", "Retro"];
    private static readonly string[] AllowedSex = ["Male", "Female", "Unisex"];
    
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private const int MaxImagesPerJersey = 10;
    private const int MaxPatchesPerJersey = 5;

    public CreateJerseyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del jersey es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

        RuleFor(x => x.IdClub)
            .GreaterThan(0).WithMessage("Debe seleccionar un club válido.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de jersey es requerido.")
            .Must(type => AllowedTypes.Contains(type))
            .WithMessage($"El tipo debe ser uno de: {string.Join(", ", AllowedTypes)}");

        RuleFor(x => x.Sex)
            .NotEmpty().WithMessage("El sexo es requerido.")
            .Must(sex => AllowedSex.Contains(sex))
            .WithMessage($"El sexo debe ser uno de: {string.Join(", ", AllowedSex)}");

        RuleFor(x => x.SizeIds)
            .NotEmpty().WithMessage("Debe seleccionar al menos una talla.")
            .Must(sizes => sizes.All(id => id > 0))
            .WithMessage("Todos los IDs de talla deben ser válidos (mayor a 0).");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("El peso debe ser mayor a 0.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("La marca es requerida.")
            .MaximumLength(100).WithMessage("La marca no puede exceder 100 caracteres.");

        RuleFor(x => x.Season)
            .NotEmpty().WithMessage("La temporada es requerida.")
            .MaximumLength(20).WithMessage("La temporada no puede exceder 20 caracteres.")
            .Matches(@"^\d{2}/\d{2}$").WithMessage("La temporada debe tener el formato XX/XX (ej: 24/25)");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");

        // Validación de imágenes
        RuleFor(x => x.Images)
            .Must(images => images.Count <= MaxImagesPerJersey)
            .WithMessage($"No se pueden subir más de {MaxImagesPerJersey} imágenes por jersey.");

        RuleForEach(x => x.Images)
            .ChildRules(image =>
            {
                image.RuleFor(i => i.FileName)
                    .NotEmpty().WithMessage("El nombre del archivo es requerido.")
                    .Must(HasValidExtension)
                    .WithMessage($"Extensión no permitida. Use: {string.Join(", ", AllowedExtensions)}");

                image.RuleFor(i => i.ContentType)
                    .NotEmpty().WithMessage("El tipo de contenido es requerido.")
                    .Must(ct => AllowedContentTypes.Contains(ct.ToLower()))
                    .WithMessage($"Tipo de contenido no permitido. Use: {string.Join(", ", AllowedContentTypes)}");

                image.RuleFor(i => i.Content)
                    .NotNull().WithMessage("El contenido del archivo es requerido.")
                    .Must(stream => stream.Length <= MaxFileSizeBytes)
                    .WithMessage($"El archivo no puede exceder {MaxFileSizeBytes / (1024 * 1024)}MB.");
            });

        // Validación de patches
        RuleFor(x => x.Patches)
            .Must(patches => patches.Count <= MaxPatchesPerJersey)
            .WithMessage($"No se pueden subir más de {MaxPatchesPerJersey} parches por jersey.");

        RuleForEach(x => x.Patches)
            .ChildRules(patch =>
            {
                // Validar el nombre del patch
                patch.RuleFor(p => p.Name)
                    .NotEmpty().WithMessage("El nombre del patch es requerido.");

                // Validar la temporada del patch
                patch.RuleFor(p => p.Season)
                    .NotEmpty().WithMessage("La temporada del patch es requerida.");

                // Validar la imagen del patch
                patch.RuleFor(p => p.Image)
                    .NotNull().WithMessage("La imagen del patch es requerida.");

                patch.RuleFor(p => p.Image.FileName)
                    .NotEmpty().WithMessage("El nombre del archivo del patch es requerido.")
                    .Must(HasValidExtension)
                    .WithMessage($"Extensión no permitida. Use: {string.Join(", ", AllowedExtensions)}")
                    .When(p => p.Image != null);

                patch.RuleFor(p => p.Image.ContentType)
                    .NotEmpty().WithMessage("El tipo de contenido del patch es requerido.")
                    .Must(ct => AllowedContentTypes.Contains(ct.ToLower()))
                    .WithMessage($"Tipo de contenido no permitido. Use: {string.Join(", ", AllowedContentTypes)}")
                    .When(p => p.Image != null);

                patch.RuleFor(p => p.Image.Content)
                    .NotNull().WithMessage("El contenido del archivo del patch es requerido.")
                    .Must(stream => stream.Length <= MaxFileSizeBytes)
                    .WithMessage($"El archivo del patch no puede exceder {MaxFileSizeBytes / (1024 * 1024)}MB.")
                    .When(p => p.Image != null);
            });

        // Validación de categorías
        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("Debe seleccionar al menos una categoría.");
    }

    private static bool HasValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}
