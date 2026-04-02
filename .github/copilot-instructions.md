# AI Coding Instructions — MyApp (.NET Clean Architecture)

## Project Overview

This is a **Clean Architecture** ASP.NET Core template with 4 layers:
- **MyApp.Domain** — Entities, Value Objects, Repository Interfaces, Domain Exceptions
- **MyApp.Application** — DTOs, Service Interfaces, Validators, Result<T> pattern
- **MyApp.Infrastructure** — EF Core, JWT, Repository implementations, External services
- **MyApp.API** — Minimal API endpoints, Middleware, Composition root

## Architecture Rules (MUST FOLLOW)

1. **Dependency direction**: Domain → Application → Infrastructure → API (inner layers NEVER reference outer layers)
2. **Domain layer has ZERO external dependencies** — no NuGet packages, no framework references
3. **Application layer** defines interfaces; **Infrastructure layer** implements them
4. **API layer** is the composition root — it wires everything via DI
5. **Never throw exceptions for expected failures** — use `Result<T>` pattern
6. **Use file-scoped namespaces** (`namespace X;` not `namespace X { }`)
7. **All entities inherit from `BaseEntity`** (Id, CreatedAt, UpdatedAt)

## Code Conventions

- **Naming**: PascalCase for public members, `_camelCase` for private fields, `I` prefix for interfaces
- **Async**: All I/O operations must be async, method names end with `Async`
- **Records**: Use `record` for DTOs and value objects
- **Nullable**: Nullable reference types are enabled — don't use `null!` unless absolutely necessary
- **Validation**: Use FluentValidation validators in the Application layer

## Adding a New Feature (Checklist)

1. **Domain**: Create entity in `Domain/Entities/`, add repository interface in `Domain/Repositories/`
2. **Application**: Create DTOs in `Application/<Feature>/DTOs/`, service interface, validators
3. **Infrastructure**: Implement repository in `Infrastructure/Persistence/Repositories/`, add EF configuration in `Configurations/`, register in `DependencyInjection.cs`
4. **API**: Add endpoint group in `API/Endpoints/`, register in `Program.cs`
5. **Tests**: Add unit tests for validators/services, integration tests for endpoints

## Technology Stack

| Component | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core + SQLite |
| Auth | JWT Bearer (access + refresh tokens) |
| Validation | FluentValidation |
| Logging | Serilog (Console, File, Seq-ready) |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit, FluentAssertions, NSubstitute |
| Package Mgmt | Central Package Management (Directory.Packages.props) |

## Key Commands

```bash
# Build
dotnet build

# Run (development)
cd src/MyApp.API && dotnet run

# Run tests
dotnet test

# Add EF migration
dotnet ef migrations add <Name> --project src/MyApp.Infrastructure --startup-project src/MyApp.API --output-dir Persistence/Migrations

# Apply migrations
dotnet ef database update --project src/MyApp.Infrastructure --startup-project src/MyApp.API
```

## Project Structure

```
dotnet/
├── Directory.Build.props          # Shared build properties
├── Directory.Packages.props       # Central NuGet versions
├── global.json                    # SDK version constraint
├── MyApp.slnx                     # Solution file
├── src/
│   ├── MyApp.Domain/              # Entities, interfaces, exceptions
│   ├── MyApp.Application/         # DTOs, service contracts, validators
│   ├── MyApp.Infrastructure/      # EF Core, JWT, implementations
│   └── MyApp.API/                 # Endpoints, middleware, Program.cs
└── tests/
    ├── MyApp.UnitTests/           # Domain + Application tests
    └── MyApp.IntegrationTests/    # API endpoint tests
```

## Common Patterns

### Result<T> Pattern
```csharp
// Returning success
return Result<AuthResponse>.Success(new AuthResponse(...));

// Returning failure
return Result<AuthResponse>.Failure("Error message");
return Result<AuthResponse>.NotFound("User not found");
return Result<AuthResponse>.Unauthorized("Invalid credentials");
```

### Endpoint Pattern
```csharp
group.MapPost("/endpoint", async (RequestDto request, IService service, CancellationToken ct) =>
{
    var result = await service.DoSomethingAsync(request, ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.Problem(result.Error, statusCode: result.StatusCode);
});
```
