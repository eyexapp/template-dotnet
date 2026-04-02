# AGENTS.md — .NET Clean Architecture API

## Project Identity

| Key | Value |
|-----|-------|
| Framework | ASP.NET Core (.NET 10) |
| Language | C# (file-scoped namespaces) |
| Category | Backend API (Clean Architecture) |
| ORM | Entity Framework Core + SQLite |
| Auth | JWT Bearer (access + refresh tokens) |
| Validation | FluentValidation |
| Logging | Serilog (Console, File, Seq-ready) |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit + FluentAssertions + NSubstitute |
| Packages | Central Package Management (Directory.Packages.props) |

---

## Architecture — 4-Layer Clean Architecture

```
src/
├── MyApp.Domain/            ← DOMAIN: Entities, Value Objects, Repo Interfaces, Exceptions
│   ├── Entities/            ← BaseEntity (Id, CreatedAt, UpdatedAt)
│   ├── Repositories/        ← Repository interfaces
│   └── Exceptions/          ← Domain-specific exceptions
├── MyApp.Application/       ← APPLICATION: DTOs, Service Interfaces, Validators, Result<T>
│   ├── <Feature>/
│   │   ├── DTOs/            ← Request/Response DTOs (records)
│   │   └── Validators/      ← FluentValidation validators
│   ├── Common/
│   │   └── Result.cs        ← Result<T> pattern
│   └── DependencyInjection.cs
├── MyApp.Infrastructure/    ← INFRASTRUCTURE: EF Core, JWT, Repository Implementations
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/  ← EF entity configurations
│   │   ├── Repositories/    ← Concrete repository implementations
│   │   └── Migrations/
│   ├── Auth/                ← JWT token service
│   └── DependencyInjection.cs
└── MyApp.API/               ← PRESENTATION: Minimal API Endpoints, Middleware, Program.cs
    ├── Endpoints/           ← Endpoint groups
    ├── Middleware/
    └── Program.cs           ← Composition root

tests/
├── MyApp.UnitTests/         ← Domain + Application tests
└── MyApp.IntegrationTests/  ← API endpoint tests
```

### Dependency Direction (INWARD ONLY)
```
Domain ← Application ← Infrastructure ← API
```

### Strict Layer Rules

| Layer | Can Reference | NEVER References |
|-------|--------------|-----------------|
| `Domain` | Nothing (zero dependencies) | Application, Infrastructure, API |
| `Application` | Domain | Infrastructure, API |
| `Infrastructure` | Domain, Application | API |
| `API` | All (composition root) | — |

---

## Adding New Code — Where Things Go

### New Feature Checklist
1. **Domain**: Entity in `Domain/Entities/`, repo interface in `Domain/Repositories/`
2. **Application**: DTOs in `Application/<Feature>/DTOs/` (records), service interface, FluentValidation validators
3. **Infrastructure**: Repository in `Infrastructure/Persistence/Repositories/`, EF config in `Configurations/`, register in `DependencyInjection.cs`
4. **API**: Endpoint group in `API/Endpoints/`, register in `Program.cs`
5. **Tests**: Unit tests for validators/services, integration tests for endpoints

### Entity Pattern
```csharp
namespace MyApp.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
// BaseEntity provides: Guid Id, DateTime CreatedAt, DateTime UpdatedAt
```

### DTO Pattern — Records
```csharp
namespace MyApp.Application.Products.DTOs;

public record CreateProductRequest(string Name, decimal Price, string? Description);
public record ProductResponse(Guid Id, string Name, decimal Price, DateTime CreatedAt);
```

### Endpoint Pattern — Minimal API
```csharp
group.MapPost("/", async (
    CreateProductRequest request,
    IProductService service,
    CancellationToken ct) =>
{
    var result = await service.CreateAsync(request, ct);
    return result.IsSuccess
        ? Results.Created($"/api/v1/products/{result.Value.Id}", result.Value)
        : Results.Problem(result.Error, statusCode: result.StatusCode);
});
```

---

## Design & Architecture Principles

### Result\<T\> Pattern — NO Exceptions for Expected Failures
```csharp
// Returning success
return Result<ProductResponse>.Success(new ProductResponse(...));

// Returning failure
return Result<ProductResponse>.Failure("Validation failed");
return Result<ProductResponse>.NotFound("Product not found");
return Result<ProductResponse>.Unauthorized("Invalid credentials");
```

### Async Everywhere
- ALL I/O operations MUST be `async`
- Method names end with `Async`: `CreateAsync`, `GetByIdAsync`
- Always accept `CancellationToken` as last parameter

### FluentValidation
```csharp
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### Nullable Reference Types
- Enabled project-wide
- Use `?` for truly nullable properties
- NEVER use `null!` unless absolutely necessary

---

## Error Handling

### Result\<T\> Flow
- Services return `Result<T>` — never throw for expected failures
- Endpoints map `Result<T>` to HTTP responses: `Results.Ok`, `Results.Problem`, `Results.NotFound`
- Domain exceptions for truly exceptional/invariant-violating cases only
- Middleware catches unhandled exceptions → 500 with generic message

### Logging
```csharp
// Serilog structured logging
_logger.LogInformation("Product created: {ProductId}", product.Id);
_logger.LogError(ex, "Failed to create product: {Name}", request.Name);
```

---

## Code Quality

### Naming Conventions
| Artifact | Convention | Example |
|----------|-----------|---------|
| Entity | `PascalCase` | `Product.cs` |
| DTO | `VerbNounRequest/Response` record | `CreateProductRequest.cs` |
| Service | `INounService` / `NounService` | `IProductService.cs` |
| Repository | `INounRepository` / `NounRepository` | `IProductRepository.cs` |
| Validator | `NounValidator` | `CreateProductValidator.cs` |
| Endpoint | `NounEndpoints` | `ProductEndpoints.cs` |
| Config | `NounConfiguration` | `ProductConfiguration.cs` |
| Migration | Auto-generated | `20240101_AddProducts.cs` |

### C# Conventions
- File-scoped namespaces: `namespace X;` (NOT `namespace X { }`)
- `_camelCase` for private fields
- `PascalCase` for public members
- `I` prefix for interfaces
- One class/record per file (within reason)

---

## Testing Strategy

| Level | What | Where | Tool |
|-------|------|-------|------|
| Unit | Validators, services, domain | `tests/MyApp.UnitTests/` | xUnit + FluentAssertions |
| Integration | API endpoints | `tests/MyApp.IntegrationTests/` | xUnit + WebApplicationFactory |

### Unit Test Pattern
```csharp
[Fact]
public async Task CreateAsync_WithValidData_ReturnsSuccess()
{
    // Arrange
    var repo = Substitute.For<IProductRepository>();
    var service = new ProductService(repo);

    // Act
    var result = await service.CreateAsync(new CreateProductRequest("Test", 10m, null), CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be("Test");
}
```

### What MUST Be Tested
- All FluentValidation validators (valid + invalid rules)
- All service methods (success + failure paths)
- All endpoint routes (status codes + response shapes)
- Domain entity invariants
- Result\<T\> mapping in endpoints

---

## Security & Performance

### Security
- JWT Bearer auth with access + refresh tokens
- FluentValidation on all input — before business logic
- EF Core parameterized queries — SQL injection impossible
- Nullable reference types — null safety at compile time
- Serilog structured logging — no sensitive data in logs

### Performance
- `CancellationToken` on all async operations — client disconnect = cancel
- EF Core query optimization: `.AsNoTracking()` for read-only queries
- Pagination via query parameters — never return full table
- Central Package Management — consistent dependency versions

---

## Commands

| Action | Command |
|--------|---------|
| Build | `dotnet build` |
| Run (dev) | `cd src/MyApp.API && dotnet run` |
| Test | `dotnet test` |
| Add migration | `dotnet ef migrations add <Name> --project src/MyApp.Infrastructure --startup-project src/MyApp.API` |
| Apply migrations | `dotnet ef database update --project src/MyApp.Infrastructure --startup-project src/MyApp.API` |

---

## Prohibitions — NEVER Do These

1. **NEVER** throw exceptions for expected failures — use `Result<T>` pattern
2. **NEVER** reference outer layers from inner layers (Domain must not reference Infrastructure)
3. **NEVER** add NuGet packages to Domain layer — zero external dependencies
4. **NEVER** use `null!` without documentation justifying it
5. **NEVER** use block-scoped namespaces — file-scoped (`namespace X;`) only
6. **NEVER** skip `CancellationToken` on async methods
7. **NEVER** skip `async` on I/O operations — everything must be async
8. **NEVER** return EF entities from APIs — use DTOs (records)
9. **NEVER** use `var` when type is not obvious from context
10. **NEVER** put business logic in endpoints — delegate to services/actions
