---
name: testing
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
  - test
  - xunit
  - moq
  - integration test
  - testcontainers
---

# Testing — .NET (xUnit + Moq + Testcontainers)

## Unit Tests (xUnit + Moq)

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsResponse()
    {
        var user = new User("Alice", new Email("alice@test.com"));
        _repoMock.Setup(r => r.GetByIdAsync(user.Id, default))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetById_NonExistent_ThrowsNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((User?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
```

## Integration Tests (WebApplicationFactory)

```csharp
public class UsersApiTests(WebApplicationFactory<Program> factory) 
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateUser_Returns201()
    {
        var request = new { Name = "Alice", Email = "alice@test.com" };
        var response = await _client.PostAsJsonAsync("/api/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## Testcontainers (DB)

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public Task InitializeAsync() => _postgres.StartAsync();
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();
}
```

## Rules

- `[Fact]` for single-case, `[Theory] [InlineData]` for parameterized.
- FluentAssertions: `result.Should().Be(expected)`.
- Moq: `Setup` + `Verify` — no real dependencies in unit tests.
- Test naming: `Method_Scenario_ExpectedResult`.
- `dotnet test --collect:"XPlat Code Coverage"` for coverage.
