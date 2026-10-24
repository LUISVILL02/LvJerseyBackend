# AGENTS.md - LvJerseyStore

This document provides guidelines for AI coding agents working on this codebase.

## Project Overview

**LvJerseyStore** is a .NET 9.0 ASP.NET Core Web API for an e-commerce jersey store.

### Technology Stack
- **Framework**: .NET 9.0 / ASP.NET Core
- **Database**: PostgreSQL with Entity Framework Core 9.0
- **Validation**: FluentValidation 12.0
- **Authentication**: JWT Bearer + Google OAuth
- **Email**: SendGrid
- **API Docs**: Swagger/Swashbuckle

### Architecture
Modular Monolith with Clean Architecture. Business modules:
- `Authentication/` - Login, Register, JWT, Social Auth, Email Verification
- `Users/` - User management, Roles, Addresses
- `Jerseys/` - Product catalog (Jerseys, Clubs, Leagues, Categories)
- `Files/` - File/image management
- `Shared/` - Cross-cutting concerns (DbContext, ISender, IEmailSender)

Each module follows: `*.Application/`, `*.Domain/`, `*.Infrastructure/` layers.

---

## Build / Run / Test Commands

```bash
# Build entire solution
dotnet build LvJerseyStore.sln

# Run the API (Development)
dotnet run --project API/LvJerseyApi/LvJerseyApi.csproj

# EF Migrations
dotnet ef migrations add <Name> --project Shared/Shared.Application --startup-project API/LvJerseyApi
dotnet ef database update --project Shared/Shared.Application --startup-project API/LvJerseyApi

# Docker
docker-compose -f compose.yaml up --build
```

### Testing (no test projects yet)
```bash
dotnet test                                      # Run all tests
dotnet test --filter "FullyQualifiedName~Name"   # Run single test
dotnet test --filter "ClassName~MyTests"         # Run tests by class
```

---

## Code Style Guidelines

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `LoginCommandHandler` |
| Interfaces | IPascalCase | `ICommandHandler`, `IJwtUtil` |
| Methods/Properties | PascalCase | `HandleAsync`, `IdUser` |
| Private fields | _camelCase | `_repository`, `_sender` |
| Parameters | camelCase | `command`, `userId` |
| DB columns | snake_case | `id_user`, `email_confirmed` |

### File & Namespace Organization
- **File-scoped namespaces**: `namespace Authentication.Application.Commands.Login;`
- **Pattern**: `{Module}.{Layer}.{Feature}`

### Import Order
1. System → 2. Microsoft → 3. Third-party → 4. Project → 5. Shared

### Type Declarations
- **Nullable enabled** - Use `string?` for optional, `string = null!` for required but late-initialized
- **Commands/Queries**: `record` with primary constructors
- **Entities**: `sealed class` with `{ get; set; }`
- **Handlers/Controllers**: `class` with primary constructor for DI

---

## Architecture Patterns

### CQRS Implementation
```csharp
// Command - Commands/{Name}/{Name}Command.cs
public record LoginCommand(string UserName, string Password) : ICommand<AuthResponseDto>;

// Handler - Commands/{Name}/{Name}CommandHandler.cs
public class LoginCommandHandler(IJwtUtil jwt, IUserAuthRepository repo) 
    : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(LoginCommand command) { ... }
}

// Dispatching via ISender
await sender.SendCommandAsync<LoginCommand, AuthResponseDto>(command);
await sender.SendQueryAsync<HomeJerseysQuery, List<Response>>(query);
```

### Module Structure
```
{Module}/
├── {Module}.Application/
│   ├── Abstractions/          # Interfaces
│   ├── Commands/{Name}/       # Command + Handler per folder
│   ├── Queries/{Name}/        # Query + Handler per folder
│   ├── Dtos/                  # Data transfer objects
│   └── Validations/           # FluentValidation validators
├── {Module}.Domain/
│   ├── Entities/              # Domain entities
│   └── Exceptions/            # Domain-specific exceptions
└── {Module}.Infrastructure/
    ├── Configurations/        # EF Core entity configs
    ├── Services/              # Interface implementations
    └── DependencyInjection/   # ServiceCollectionExtensions.cs
```

### Entity Configuration (EF Core)
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.IdUser).HasName("Users_pkey");
        builder.Property(e => e.IdUser).HasColumnName("id_user");  // snake_case
        builder.Property(e => e.Email).HasColumnType("character varying").HasColumnName("email");
    }
}
```

### Validation Pattern
```csharp
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
```

---

## Error Handling

- **Domain exceptions** in `{Module}.Domain/Exceptions/`:
  ```csharp
  public class UserFoundException(string message) : Exception(message);
  ```
- Global handler in `Program.cs` maps exceptions → HTTP status codes
- `FluentValidation.ValidationException` → 400 with error details

---

## API Conventions

```csharp
[ApiController]
[Route("api/v0.0.1/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost, Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
    {
        var response = await sender.SendCommandAsync<LoginCommand, AuthResponseDto>(cmd);
        return Ok(response);
    }
}
```

---

---

## Pipeline de 3 Subagentes (Orquestación)

Usa este pipeline para tareas complejas. Los 3 subagentes trabajan en secuencia: **Analizar → Ejecutar → Testear**.

### Cómo invocar el pipeline completo

Cuando recibas una tarea del usuario, ejecuta la secuencia cargando cada skill con `skill` tool y lanzando un subagente via `Task` tool:

```
Paso 1: Cargar skill subagent-analyst → Task subagent_type="explore" con el plan
Paso 2: Cargar skill subagent-executor → Task subagent_type="general" con el plan del paso 1
Paso 3: Cargar skill subagent-tester → Task subagent_type="general" con la impl del paso 2
```

### Agente 1 — Analyst (Planificador)
- **Skill**: `subagent-analyst`
- **Subagent type**: `explore` (solo lectura)
- **Output**: Plan detallado con archivos a crear/modificar, pasos ordenados, dependencias
- **NO** modifica ningún archivo

### Agente 2 — Executor (Implementador)
- **Skill**: `subagent-executor`
- **Subagent type**: `general` (lectura/escritura)
- **Input**: Plan del Analyst
- **Output**: Archivos creados/modificados, verificación de compilación

### Agente 3 — Tester (Probador)
- **Skill**: `subagent-tester`
- **Subagent type**: `general` (lectura/escritura)
- **Input**: Plan del Analyst + implementación del Executor
- **Output**: Tests unitarios + integración, verificación de `dotnet test`

### Verificación final (orquestador)
Después de que los 3 agentes completen:
```bash
dotnet build LvJerseyStore.sln
dotnet test
```

---

## Important Notes

1. **No test projects exist** - Consider adding when implementing new features
2. **appsettings.Development.json** - Contains secrets, never commit real credentials
3. **Primary constructors** preferred for DI in .NET 9
4. **Spanish language** in some error messages/comments
5. **Entry point**: `API/LvJerseyApi/Program.cs`
6. **DbContext**: `Shared.Infrastructure/Data/ApplicationDbContext.cs`
