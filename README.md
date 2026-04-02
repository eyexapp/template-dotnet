# MyApp — .NET Clean Architecture Template

A production-ready **ASP.NET Core** template using **Clean Architecture**, JWT authentication, EF Core, and comprehensive testing.

## Architecture

```
src/
├── MyApp.Domain           # Entities, Value Objects, Interfaces, Exceptions
├── MyApp.Application      # DTOs, Service Contracts, Validators, Result<T>
├── MyApp.Infrastructure   # EF Core, JWT, Repository implementations
└── MyApp.API              # Minimal API endpoints, Composition root

tests/
├── MyApp.UnitTests        # Domain + Application layer tests
└── MyApp.IntegrationTests # API endpoint tests (WebApplicationFactory)
```

**Dependency flow**: Domain → Application → Infrastructure → API

## Tech Stack

- **.NET 10** (ASP.NET Core Minimal API)
- **EF Core + SQLite** (code-first, migrations)
- **JWT Bearer Auth** (access + refresh tokens, BCrypt hashing)
- **FluentValidation** (input validation)
- **Serilog** (structured logging — Console, File, Seq-ready)
- **Scalar** (modern OpenAPI documentation UI)
- **xUnit + FluentAssertions + NSubstitute** (testing)
- **Central Package Management** (Directory.Packages.props)

## Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Run the API (development mode)
cd src/MyApp.API && dotnet run

# Open in browser
# API Docs:  http://localhost:5276/scalar/v1
# Health:    http://localhost:5276/health
```

## Key Commands

```bash
# Build entire solution
dotnet build

# Run all tests (unit + integration)
dotnet test

# Run with hot reload
cd src/MyApp.API && dotnet watch

# Add a migration
dotnet ef migrations add <Name> \
  --project src/MyApp.Infrastructure \
  --startup-project src/MyApp.API \
  --output-dir Persistence/Migrations

# Apply migrations
dotnet ef database update \
  --project src/MyApp.Infrastructure \
  --startup-project src/MyApp.API
```

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get JWT tokens |
| POST | `/api/auth/refresh` | Refresh an expired access token |
| GET | `/health` | Health check |
| GET | `/` | Redirect to Scalar API docs |

## Adding a New Feature

1. **Domain** — Create entity in `Entities/`, repository interface in `Repositories/`
2. **Application** — Create DTOs, service interface, FluentValidation validators
3. **Infrastructure** — Implement repository, add EF configuration, register in DI
4. **API** — Add endpoint group, register in `Program.cs`
5. **Tests** — Unit tests for logic, integration tests for endpoints

## Project Conventions

- **Result\<T\>** pattern for expected failures (no exception throwing)
- **File-scoped namespaces** everywhere
- All entities inherit **BaseEntity** (Id, CreatedAt, UpdatedAt)
- All I/O is **async** (method names end with `Async`)
- NuGet versions centralized in **Directory.Packages.props**
- Test naming: `MethodName_Scenario_ExpectedBehavior`

## License

MIT