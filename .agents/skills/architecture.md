---
name: architecture
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
  - architecture
  - dotnet
  - asp.net core
  - clean architecture
  - ef core
---

# Architecture — ASP.NET Core (.NET 10) Clean Architecture

## Project Structure

```
src/
├── Api/                           ← Host + Controllers
│   ├── Program.cs                 ← Minimal hosting, DI config
│   ├── Controllers/
│   │   └── UsersController.cs
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs
│   └── Api.csproj
├── Application/                   ← Use cases + interfaces
│   ├── Users/
│   │   ├── Commands/
│   │   │   └── CreateUserCommand.cs
│   │   ├── Queries/
│   │   │   └── GetUserQuery.cs
│   │   └── Validators/
│   │       └── CreateUserValidator.cs
│   ├── Common/
│   │   └── IRepository.cs
│   └── Application.csproj
├── Domain/                        ← Entities + value objects
│   ├── Entities/
│   │   └── User.cs
│   ├── ValueObjects/
│   │   └── Email.cs
│   └── Domain.csproj
└── Infrastructure/                ← EF Core, external services
    ├── Persistence/
    │   ├── AppDbContext.cs
    │   ├── Configurations/
    │   │   └── UserConfiguration.cs
    │   └── Repositories/
    │       └── UserRepository.cs
    └── Infrastructure.csproj
```

## Dependency Flow

```
Api → Application → Domain
Api → Infrastructure → Application
Infrastructure → Domain
```

## Minimal API Registration (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.Run();
```

## MediatR CQRS Pattern

```csharp
// Command
public record CreateUserCommand(string Name, string Email) : IRequest<UserResponse>;

public class CreateUserHandler(IUserRepository repo) : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var user = new User(cmd.Name, new Email(cmd.Email));
        await repo.AddAsync(user, ct);
        return UserResponse.From(user);
    }
}
```

## EF Core Configuration

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.OwnsOne(u => u.Email);
    }
}
```

## Rules

- Clean Architecture: Domain has zero dependencies.
- MediatR for CQRS — commands and queries separated.
- FluentValidation for request validation.
- EF Core with explicit `IEntityTypeConfiguration`.
- Migrations via `dotnet ef` CLI.
