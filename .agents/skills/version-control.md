---
name: version-control
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
  - git
  - commit
  - ci
  - docker
  - deploy
---

# Version Control — .NET

## Commits (Conventional)

- `feat(users): add CQRS command for user creation`
- `fix(auth): validate refresh token expiry`
- `db: add migration for orders table`

## CI Pipeline

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --collect:"XPlat Code Coverage"
dotnet publish -c Release -o ./publish
```

## Docker (Multi-stage)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.sln .
COPY src/Api/Api.csproj src/Api/
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
RUN dotnet restore
COPY . .
RUN dotnet publish src/Api -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
```

## EF Core Migrations

```bash
dotnet ef migrations add AddOrdersTable -p src/Infrastructure -s src/Api
dotnet ef database update -s src/Api
```

## .gitignore

```
bin/
obj/
publish/
.vs/
*.user
appsettings.Development.json
```

## Configuration

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=myapp;Username=postgres;Password=postgres"
  }
}

// appsettings.Production.json — env vars override
// ConnectionStrings__Default from environment
```
