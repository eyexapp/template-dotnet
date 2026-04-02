---
name: code-quality
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
  - code quality
  - naming
  - fluent validation
  - records
  - patterns
---

# Code Quality — .NET 10 + ASP.NET Core

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Class | PascalCase | `UserService` |
| Interface | I + PascalCase | `IUserRepository` |
| Method | PascalCase | `GetByIdAsync()` |
| Property | PascalCase | `CreatedAt` |
| Private field | _camelCase | `_userRepository` |
| Local variable | camelCase | `userCount` |
| Constant | PascalCase | `MaxPageSize` |
| Namespace | PascalCase | `App.Application.Users` |

## Primary Constructors (C# 12)

```csharp
// ✅ Primary constructor for DI
public class UserService(IUserRepository repo, ILogger<UserService> logger)
{
    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        logger.LogInformation("Getting user {Id}", id);
        var user = await repo.GetByIdAsync(id) 
            ?? throw new NotFoundException($"User {id} not found");
        return UserResponse.From(user);
    }
}
```

## DTOs as Records

```csharp
public record CreateUserRequest(string Name, string Email);
public record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt)
{
    public static UserResponse From(User entity) =>
        new(entity.Id, entity.Name, entity.Email.Value, entity.CreatedAt);
}
```

## FluentValidation

```csharp
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

// Register in DI
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
```

## Result Pattern

```csharp
public class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new() { Value = value };
    public static Result<T> Failure(Error error) => new() { Error = error };
}
```

## Structured Logging (Serilog)

```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Usage — structured, not string interpolation
logger.LogInformation("User {UserId} created at {Timestamp}", user.Id, DateTime.UtcNow);
```
