using Authentication.Application.DependencyInjection;
using Authentication.Domain.Exceptions;
using LvJersey.Infrastructure;
using LvJerseyApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Shared.Infrastructure.DependencyInjection;
using Users.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddInfrastructure(configuration);
builder.Services.AddAuthenticationModule();
builder.Services.AddSharedInfrastructure();
builder.Services.AddUsersModule();


builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SIGEI API", Version = "v1" });

    // Configuración para usar JWT en Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingresa tu JWT así: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var status = context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            UserFoundException => StatusCodes.Status400BadRequest,
            AggregateException => StatusCodes.Status400BadRequest,
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        context.Response.StatusCode = status;
        
        if (exception is FluentValidation.ValidationException validationEx)
        {
            var errors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var response = new ResponseException(
                status,
                "Errores de validación",
                errors);

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        if (exception is AggregateException aggregateEx)
        {
            var errors = aggregateEx.InnerExceptions
                .GroupBy(e => e.GetType().Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Message).ToArray()
                );
            var response = new ResponseException(
                status,
                "Hubo algunos errores",
                errors);

            await context.Response.WriteAsJsonAsync(response);
            return;
        }
        
        var generic = new ResponseException(
            status,
            exception?.Message ?? "Error interno",
            new Dictionary<string, string[]>()
            );

        await context.Response.WriteAsJsonAsync(generic);
    });    
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();