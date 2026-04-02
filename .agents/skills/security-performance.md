---
name: security-performance
type: knowledge
version: 1.0.0
agent: CodeActAgent
triggers:
  - security
  - performance
  - jwt
  - ef core optimization
  - caching
---

# Security & Performance — .NET + ASP.NET Core

## Performance

### EF Core Query Optimization

```csharp
// Projections — only fetch needed columns
var users = await context.Users
    .Where(u => u.IsActive)
    .Select(u => new UserSummary(u.Id, u.Name))
    .ToListAsync(ct);

// AsNoTracking for read-only
var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

// Split queries for collections
var orders = await context.Users
    .Include(u => u.Orders)
    .AsSplitQuery()
    .ToListAsync(ct);
```

### Response Caching

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("UserById", b => b.SetVaryByRouteValue("id").Expire(TimeSpan.FromMinutes(10)));
});

[OutputCache(PolicyName = "UserById")]
public async Task<UserResponse> GetById(Guid id) { ... }
```

### Async Everywhere

```csharp
// ✅ Async all the way
public async Task<List<UserResponse>> GetAllAsync(CancellationToken ct)
{
    return await context.Users.Select(u => UserResponse.From(u)).ToListAsync(ct);
}

// ❌ Never .Result or .Wait() — deadlock risk
```

## Security

### JWT Authentication

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

### Authorization Policies

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
    .AddPolicy("CanEditUser", p => p.RequireClaim("user:write"));

[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> DeleteUser(Guid id) { ... }
```

### Input Validation

- FluentValidation for complex rules (registered in pipeline).
- Model binding with `[Required]` for simple cases.
- Always validate at controller entry via `[ApiController]` auto-validation.

### CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://myapp.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Secrets

- `dotnet user-secrets` for local development.
- Azure Key Vault / environment variables in production.
- Never hardcode connection strings.
