using FluentAssertions;
using FluentValidation.TestHelper;
using Jerseys.Application.Commands.CreateJersey;
using Jerseys.Application.Dtos;
using Jerseys.Application.Validations;

namespace LvJerseyStore.UnitTests.Jerseys;

public class CreateJerseyCommandValidatorTests
{
    private readonly CreateJerseyCommandValidator _validator;

    public CreateJerseyCommandValidatorTests()
    {
        _validator = new CreateJerseyCommandValidator();
    }

    private static CreateJerseyCommand CreateValidCommand()
    {
        return new CreateJerseyCommand(
            Name: "Real Madrid Home 24/25",
            IdClub: 1,
            Type: "Player",
            Sex: "Male",
            SizeIds: [1, 2, 3],
            Weight: 0.3m,
            Brand: "Adidas",
            Season: "24/25",
            Price: 89.99m,
            Stock: 100,
            Categories: ["Primera División"],
            Images: [],
            Patches: []);
    }

    /// <summary>
    /// Test 1: Verifica que un comando válido pasa todas las validaciones
    /// </summary>
    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Test 2: Verifica validación de campos requeridos
    /// </summary>
    [Theory]
    [InlineData("", "El nombre del jersey es requerido.")]
    [InlineData(null, "El nombre del jersey es requerido.")]
    public void Validate_WhenNameIsEmpty_ShouldHaveValidationError(string? name, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand() with { Name = name! };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(expectedMessage);
    }

    /// <summary>
    /// Test 3: Verifica que el formato de temporada sea correcto (XX/XX)
    /// </summary>
    [Theory]
    [InlineData("24/25", true)]
    [InlineData("23/24", true)]
    [InlineData("2024/2025", false)]
    [InlineData("24-25", false)]
    [InlineData("invalid", false)]
    public void Validate_SeasonFormat_ShouldValidateCorrectly(string season, bool shouldBeValid)
    {
        // Arrange
        var command = CreateValidCommand() with { Season = season };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (shouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Season);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Season)
                .WithErrorMessage("La temporada debe tener el formato XX/XX (ej: 24/25)");
        }
    }

    /// <summary>
    /// Test 4: Verifica validación de tipos de archivo de imagen permitidos
    /// </summary>
    [Theory]
    [InlineData("image.jpg", "image/jpeg", true)]
    [InlineData("image.jpeg", "image/jpeg", true)]
    [InlineData("image.png", "image/png", true)]
    [InlineData("image.webp", "image/webp", true)]
    [InlineData("image.gif", "image/gif", false)]
    [InlineData("image.bmp", "image/bmp", false)]
    [InlineData("document.pdf", "application/pdf", false)]
    public void Validate_ImageFileType_ShouldValidateCorrectly(
        string fileName, string contentType, bool shouldBeValid)
    {
        // Arrange
        var imageStream = new MemoryStream(new byte[1024]); // 1KB file
        var images = new List<ImageUploadDto>
        {
            new(fileName, contentType, imageStream)
        };
        var command = CreateValidCommand() with { Images = images };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (shouldBeValid)
        {
            result.ShouldNotHaveAnyValidationErrors();
        }
        else
        {
            result.Errors.Should().NotBeEmpty();
        }
    }

    /// <summary>
    /// Test 5: Verifica que no se excedan los límites de imágenes (máximo 10)
    /// </summary>
    [Fact]
    public void Validate_WhenTooManyImages_ShouldHaveValidationError()
    {
        // Arrange
        var images = Enumerable.Range(1, 11)
            .Select(i => new ImageUploadDto(
                $"image{i}.jpg",
                "image/jpeg",
                new MemoryStream(new byte[1024])))
            .ToList();
        
        var command = CreateValidCommand() with { Images = images };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Images)
            .WithErrorMessage("No se pueden subir más de 10 imágenes por jersey.");
    }

    /// <summary>
    /// Test adicional: Verifica validación de valores permitidos para Type
    /// </summary>
    [Theory]
    [InlineData("Player", true)]
    [InlineData("Fan", true)]
    [InlineData("Retro", true)]
    [InlineData("Invalid", false)]
    [InlineData("", false)]
    public void Validate_JerseyType_ShouldValidateCorrectly(string type, bool shouldBeValid)
    {
        // Arrange
        var command = CreateValidCommand() with { Type = type };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (shouldBeValid)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Type);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Type);
        }
    }
}
