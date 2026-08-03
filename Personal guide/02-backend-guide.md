# Backend Guide — Step by Step (.NET 10, EF Core code-first)

Builds milestones M2, M3, M5, M6 from [00-how-to-use-this-guide.md](00-how-to-use-this-guide.md). Each step teaches one pattern; from M7 you repeat them.

**How every step is written, so nothing is left implicit:**
- Every new file states its **exact folder path**, how to create that folder/file in Visual Studio, and the **complete file contents** — full `using` statements and `namespace`, not just a fragment. Type it exactly; you can always trim things you understand later.
- Every edit to an *existing* file (`Program.cs`, `appsettings.json`) shows the surrounding lines so you know precisely where the new code goes.
- "VS:" lines are the exact Solution Explorer clicks. If you prefer the terminal for file creation, a plain `New-Item` works too — VS's "Add → Class" just saves you typing the boilerplate `namespace` line.

---

## Step 1 — Solution skeleton (M2 begins)

*Concept in one line:* the three projects enforce the [doc 11](<../AI Guide/11-coding-standards.md>) dependency rule mechanically — Domain can't secretly use EF because it literally has no reference to it.

Do this in a **terminal** (PowerShell), standing in `H:\FarmApp` — there's no solution yet for VS to open:

```powershell
dotnet new sln -n FarmApp
dotnet new classlib -n FarmApp.Domain -o src\FarmApp.Domain
dotnet new classlib -n FarmApp.Infrastructure -o src\FarmApp.Infrastructure
dotnet new webapi   -n FarmApp.Api    -o src\FarmApp.Api --use-controllers
dotnet sln add src\FarmApp.Domain src\FarmApp.Infrastructure src\FarmApp.Api

# references: Api → Infrastructure → Domain (one direction only)
dotnet add src\FarmApp.Infrastructure reference src\FarmApp.Domain
dotnet add src\FarmApp.Api reference src\FarmApp.Infrastructure
dotnet build
```

Then, still in `H:\FarmApp`:

```powershell
dotnet new gitignore
New-Item LEARNING.md, DECISIONS.md -ItemType File
```

Open `DECISIONS.md` in any editor and paste in the "Decisions so far" list from [AI Guide README](<../AI Guide/README.md>) — that's your seed.

Now open `H:\FarmApp\FarmApp.sln` in Visual Studio for everything from here on.

✅ **Checkpoint:** `dotnet build` says Build succeeded, 0 warnings worth worrying about. Commit: `"M2.1 solution skeleton"`.

## Step 2 — First entity + DbContext

*Concept:* an **entity** is a plain C# class EF maps to a table. The **DbContext** is the gateway: one `DbSet<T>` per table, plus configuration. Start with the smallest table in [doc 02](<../AI Guide/02-data-model.md>) — `Grade` — because the pattern matters, not the table.

### 2.1 — The entity

**VS:** right-click **FarmApp.Domain** → Add → New Folder → name it `Entities`. Right-click `Entities` → Add → Class → name it `Grade.cs`.

Replace the file's contents with exactly this:

```csharp
namespace FarmApp.Domain.Entities;

public class Grade
{
    public int GradeId { get; set; }
    public string Name { get; set; } = null!;
}
```

### 2.2 — Install EF packages

**VS:** right-click **FarmApp.Infrastructure** → Manage NuGet Packages → Browse → install `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Design`. Then right-click **FarmApp.Api** → Manage NuGet Packages → install `Microsoft.EntityFrameworkCore.Design` there too.

Or, in a terminal from `H:\FarmApp`:
```powershell
dotnet add src\FarmApp.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src\FarmApp.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src\FarmApp.Api package Microsoft.EntityFrameworkCore.Design
```

> **Version pitfall:** `dotnet add package` grabs the *latest* version, and latest EF (10.x) only runs on `net10.0`. If NuGet refuses with **NU1202 "not compatible"**, your projects are still targeting `net9.0` — open each `.csproj` and change `<TargetFramework>net9.0</TargetFramework>` to `net10.0`, then retry. General lesson: NU1202 always means package .NET version ≠ project .NET version.

### 2.3 — The DbContext

**VS:** right-click **FarmApp.Infrastructure** → Add → New Folder → `Persistence`. Right-click `Persistence` → Add → Class → `FarmAppDbContext.cs`.

```csharp
using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence;

public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options) : DbContext(options)
{
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfigurationsFromAssembly(typeof(FarmAppDbContext).Assembly);
}
```

*What's happening:* `DbContextOptions<FarmAppDbContext> options` is a **primary constructor** — shorthand for a normal constructor that just forwards `options` to the base `DbContext`. `DbSet<Grade> Grades` is the one line that says "there is a Grades table"; you'll query through it later. `ApplyConfigurationsFromAssembly` auto-discovers every `IEntityTypeConfiguration<T>` class in this project — that's what the next file plugs into, with zero manual registration.

### 2.4 — The entity configuration ([doc 11](<../AI Guide/11-coding-standards.md>) rule: fluent config, not attributes)

**VS:** right-click `Persistence` → Add → New Folder → `Configurations`. Right-click `Configurations` → Add → Class → `GradeConfiguration.cs`.

```csharp
using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> b)
    {
        b.HasKey(x => x.GradeId);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}
```

### 2.5 — Connection string + DI registration

**VS:** in Solution Explorer, expand **FarmApp.Api**, double-click `appsettings.Development.json`. Add the `ConnectionStrings` block (the whole file should now read):

```json
{
  "ConnectionStrings": {
    "FarmApp": "Server=(localdb)\\MSSQLLocalDB;Database=FarmApp;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Then double-click `Program.cs` (in **FarmApp.Api**). Add two `using` lines at the very top, and one registration before `builder.Services.AddControllers();`:

```csharp
using FarmApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<FarmAppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("FarmApp")));

builder.Services.AddControllers();
// ...the rest of the template's Program.cs stays as-is below this point
```

✅ **Checkpoint:** `dotnet build` (or Ctrl+Shift+B in VS) succeeds. You can explain: entity = row shape, DbSet = table handle, configuration = column rules.

## Step 3 — First migration (M2 done)

*Concept:* a **migration** is a generated C# file describing schema changes; `database update` runs it as SQL. Your schema history becomes source-controlled files — that's the whole magic of code-first.

### 3.1 — Install the EF CLI tool (one-time, machine-wide)

```powershell
dotnet tool install --global dotnet-ef
```

### 3.2 — Make the two commands short: wrapper scripts

Repeated `--project`/`--startup-project` flags get old fast. **VS:** right-click the **Solution** (top of Solution Explorer, not a project) → Open Folder in File Explorer → create these four files directly in `H:\FarmApp` (the folder containing `FarmApp.sln`) using any text editor (Notepad, VS Code, or VS's own File → New → File):

`H:\FarmApp\ef-add.ps1`
```powershell
param(
    [Parameter(Mandatory = $true)]
    [string]$Name
)

dotnet ef migrations add $Name `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
```

`H:\FarmApp\ef-update.ps1`
```powershell
param(
    [string]$MigrationName
)

dotnet ef database update $MigrationName `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
```

`H:\FarmApp\ef-remove.ps1`
```powershell
dotnet ef migrations remove `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
```

`H:\FarmApp\ef-list.ps1`
```powershell
dotnet ef migrations list `
    --project src\FarmApp.Infrastructure `
    --startup-project src\FarmApp.Api
```

> If PowerShell refuses to run them ("execution of scripts is disabled"): `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` once, in an admin-or-not PowerShell window — this only affects scripts you write yourself, not downloaded ones.

### 3.3 — Generate and apply

From `H:\FarmApp`:
```powershell
.\ef-add.ps1 InitialCreate
.\ef-update.ps1
```

This writes new files into `src\FarmApp.Infrastructure\Migrations\` (EF creates that folder itself — you never create it by hand) and then runs the generated SQL against `(localdb)\MSSQLLocalDB`.

Open the generated file under `Migrations\` and *read it* — see your `HasMaxLength(50)` become `maxLength: 50`. Then in SSMS: refresh Databases → `FarmApp` exists → `Grade` + `__EFMigrationsHistory` tables (SQL guide step 4).

✅ **Checkpoint:** FarmApp DB visible in SSMS with the Grade table shaped exactly as configured. Commit: `"M2.3 initial migration"` (include the two new `.ps1` files and the four migration helper scripts — they're project tooling, not build output).

## Step 4 — Repository + Unit of Work (the pattern you'll reuse forever)

*Concept:* the repository hides EF behind an interface **owned by Domain** (dependency inversion — the D in SOLID). Services say *what* they need; Infrastructure knows *how*.

### 4.1 — The interfaces (in Domain — no EF reference needed here)

**VS:** right-click **FarmApp.Domain** → Add → New Folder → `Repositories`. Right-click `Repositories` → Add → Class → `IGradeRepository.cs`.

```csharp
using FarmApp.Domain.Entities;

namespace FarmApp.Domain.Repositories;

public interface IGradeRepository
{
    Task<Grade?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Grade>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Grade grade, CancellationToken ct);
    void Remove(Grade grade);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

(Two small interfaces sharing one file is a deliberate exception to "one type per file" — `IUnitOfWork` is tiny and every future repository will use it. Split it out into its own file later if it bothers you.)

### 4.2 — The implementation (in Infrastructure)

**VS:** right-click `Persistence` (inside **FarmApp.Infrastructure**) → Add → New Folder → `Repositories`. Right-click that `Repositories` folder → Add → Class → `GradeRepository.cs`.

```csharp
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence.Repositories;

public class GradeRepository(FarmAppDbContext db) : IGradeRepository
{
    public Task<Grade?> GetByIdAsync(int id, CancellationToken ct)
        => db.Grades.FirstOrDefaultAsync(x => x.GradeId == id, ct);

    public Task<List<Grade>> GetAllAsync(CancellationToken ct)
        => db.Grades.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Grade grade, CancellationToken ct)
        => await db.Grades.AddAsync(grade, ct);

    public void Remove(Grade grade)
        => db.Grades.Remove(grade);
}
```

`AsNoTracking()` on the read-all query is the [doc 11](<../AI Guide/11-coding-standards.md>) rule — reads don't need EF's change-tracking overhead.

### 4.3 — Make the DbContext double as the Unit of Work

**VS:** open `FarmAppDbContext.cs` (from step 2.3) and change **only** the class declaration line and the top `using`s — add one `using` and append `, IUnitOfWork` to the class:

```csharp
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Infrastructure.Persistence;

public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfigurationsFromAssembly(typeof(FarmAppDbContext).Assembly);
}
```

Nothing else changes — `DbContext` already has a method `Task<int> SaveChangesAsync(CancellationToken)`, which is exactly what `IUnitOfWork` asks for. Adding `IUnitOfWork` to the class just tells the compiler "this class also satisfies that contract" — no new code needed.

### 4.4 — Wire it up for dependency injection

**VS:** open `Program.cs` (**FarmApp.Api**). Add a `using` and two registration lines, placed *after* the `AddDbContext` line from step 2.5 and *before* `AddControllers()`:

```csharp
using FarmApp.Domain.Repositories;
using FarmApp.Infrastructure.Persistence;
using FarmApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FarmAppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("FarmApp")));

builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FarmAppDbContext>());

builder.Services.AddControllers();
```

*Why bother when EF exists?* Testability (fake `IGradeRepository`, no DB needed) and one place per aggregate where queries live. And per [doc 11](<../AI Guide/11-coding-standards.md>): reports will *bypass* repositories on purpose — repositories are for transactional work.

✅ **Checkpoint:** `dotnet build` succeeds; you can explain aloud why Domain compiles without an EF package reference anywhere in `FarmApp.Domain.csproj`.

## Step 5 — First controller: Grades CRUD (M3)

*Concept:* controller = HTTP translator. DTOs in, DTOs out; entities never cross the wire ([doc 11](<../AI Guide/11-coding-standards.md>)).

### 5.1 — DTOs

**VS:** right-click **FarmApp.Api** → Add → New Folder → `Features`. Right-click `Features` → Add → New Folder → `Grades`. Right-click `Grades` → Add → Class → `GradeDtos.cs`.

```csharp
namespace FarmApp.Api.Features.Grades;

public record GradeDto(int GradeId, string Name);

public record CreateGradeRequest(string Name);
```

### 5.2 — Validation (plain FluentValidation — no auto-magic package, so it always works regardless of version)

**VS:** right-click **FarmApp.Api** → Manage NuGet Packages → install `FluentValidation`.

Right-click the `Grades` folder → Add → Class → `CreateGradeRequestValidator.cs`.

```csharp
using FluentValidation;

namespace FarmApp.Api.Features.Grades;

public class CreateGradeRequestValidator : AbstractValidator<CreateGradeRequest>
{
    public CreateGradeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}
```

Register it in `Program.cs` (one more line, anywhere before `builder.Build()`):
```csharp
using FluentValidation;
using FarmApp.Api.Features.Grades;

builder.Services.AddScoped<IValidator<CreateGradeRequest>, CreateGradeRequestValidator>();
```

### 5.3 — The controller

Right-click the `Grades` folder → Add → Class → `GradesController.cs`.

```csharp
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Features.Grades;

[ApiController]
[Route("api/v1/[controller]")]
public class GradesController(IGradeRepository repo, IUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GradeDto>>> GetAll(CancellationToken ct)
    {
        var grades = await repo.GetAllAsync(ct);
        return grades.Select(g => new GradeDto(g.GradeId, g.Name)).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GradeDto>> GetById(int id, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);
        return grade is null ? NotFound() : new GradeDto(grade.GradeId, grade.Name);
    }

    [HttpPost]
    public async Task<ActionResult<GradeDto>> Create(
        CreateGradeRequest request, IValidator<CreateGradeRequest> validator, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(e.PropertyName, e.ErrorMessage);
            return ValidationProblem(ModelState);
        }

        var grade = new Grade { Name = request.Name };
        await repo.AddAsync(grade, ct);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = grade.GradeId }, new GradeDto(grade.GradeId, grade.Name));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateGradeRequest request, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);
        if (grade is null) return NotFound();
        grade.Name = request.Name;
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var grade = await repo.GetByIdAsync(id, ct);
        if (grade is null) return NotFound();
        repo.Remove(grade);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
```

### 5.4 — Run it and click around

**VS:** press **F5** (or the green ▶ Run button with `https` selected). A browser opens; your .NET 10 template ships `AddOpenApi()` without a visual Swagger page, so also install a UI for it:

```powershell
dotnet add src\FarmApp.Api package Swashbuckle.AspNetCore
```

In `Program.cs`, inside the existing `if (app.Environment.IsDevelopment())` block, add:
```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "FarmApp API"));
}
```
Run again, then browse to `https://localhost:<port>/swagger`. Exercise every verb: create "Class 1", list it, rename it, delete it, and try posting an empty name to see the 400. Watch rows appear in SSMS as you go.

✅ **Checkpoint:** full CRUD via Swagger, validation errors are clean 400s. Commit: `"M3 grades CRUD"`. **This step is the template for every master-data table in the app** — Block and Crop follow exactly this shape (entity → config → migration → repository interface → implementation → DI → DTOs → validator → controller).

## Step 6 — Pause: wire the frontend (M4)

Jump to [03-frontend-guide.md](03-frontend-guide.md) steps 1–5 and get Angular listing your grades. Reason: seeing the full loop FE → API → DB early changes how you think about everything after; and you'll hit CORS now, with the simplest possible setup to debug it in (the FE guide covers the fix).

## Step 7 — Authentication & authorization (M5) — spec: [doc 13](<../AI Guide/13-auth-and-logging.md>) Part A

Order matters here; each sub-step is testable.

### 7.1 — Entities + configuration

**VS:** in `FarmApp.Domain\Entities\`, add `AppUser.cs`:
```csharp
namespace FarmApp.Domain.Entities;

public class AppUser
{
    public int AppUserId { get; set; }
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;   // "Owner" | "Cashier" | "Worker"
    public bool IsActive { get; set; } = true;
}
```

Add `RefreshToken.cs` in the same folder:
```csharp
namespace FarmApp.Domain.Entities;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public int AppUserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
```

In `FarmApp.Infrastructure\Persistence\Configurations\`, add `AppUserConfiguration.cs`:
```csharp
using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.HasKey(x => x.AppUserId);
        b.Property(x => x.UserName).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.UserName).IsUnique();
        b.Property(x => x.Role).HasMaxLength(20).IsRequired();
    }
}
```

And `RefreshTokenConfiguration.cs`:
```csharp
using FarmApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(x => x.RefreshTokenId);
        b.Property(x => x.Token).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Token).IsUnique();
    }
}
```

Add the two `DbSet`s to `FarmAppDbContext.cs` (alongside the existing `Grades` one):
```csharp
public DbSet<AppUser> Users => Set<AppUser>();
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
```

Migrate:
```powershell
.\ef-add.ps1 AddAuth
.\ef-update.ps1
```

### 7.2 — Password hashing

```powershell
dotnet add src\FarmApp.Api package Microsoft.Extensions.Identity.Core
```

This gives you `PasswordHasher<AppUser>` (namespace `Microsoft.AspNetCore.Identity`) — `HashPassword(user, plainText)` and `VerifyHashedPassword(user, hash, plainText)`. Never write your own hashing; this one handles salting and safe comparison.

### 7.3 — Seed the first Owner user

**VS:** in `Program.cs`, after `var app = builder.Build();` and before `app.Run();`, add a small startup block:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
    if (!db.Users.Any())
    {
        var hasher = new PasswordHasher<AppUser>();
        var owner = new AppUser { UserName = "andri", Role = "Owner" };
        owner.PasswordHash = hasher.HashPassword(owner, builder.Configuration["SeedOwnerPassword"] ?? "ChangeMe123!");
        db.Users.Add(owner);
        db.SaveChanges();
    }
}
```
Add `using FarmApp.Domain.Entities;` and `using Microsoft.AspNetCore.Identity;` to the top of `Program.cs`. Set the real seed password via user-secrets (never in a committed file):
```powershell
dotnet user-secrets init --project src\FarmApp.Api
dotnet user-secrets set "SeedOwnerPassword" "pick-a-real-password-here" --project src\FarmApp.Api
```

### 7.4 — JWT issuing (login endpoint)

```powershell
dotnet add src\FarmApp.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

Set the signing key via user-secrets (≥ 32 characters — anything shorter throws at runtime):
```powershell
dotnet user-secrets set "Jwt:SigningKey" "a-random-string-of-at-least-32-characters" --project src\FarmApp.Api
dotnet user-secrets set "Jwt:Issuer" "FarmApp" --project src\FarmApp.Api
```

**VS:** right-click `Features` (in **FarmApp.Api**) → Add → New Folder → `Auth`. Add → Class → `TokenService.cs`:
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmApp.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FarmApp.Api.Features.Auth;

public class TokenService(IConfiguration config)
{
    public string CreateAccessToken(AppUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.AppUserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken()
    {
        var bytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
```

Add → Class (still in `Features\Auth\`) → `AuthDtos.cs`:
```csharp
namespace FarmApp.Api.Features.Auth;

public record LoginRequest(string UserName, string Password);
public record RefreshRequest(string RefreshToken);
public record TokenResponse(string AccessToken, string RefreshToken);
```

Add → Class → `AuthController.cs`:
```csharp
using FarmApp.Domain.Entities;
using FarmApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.Api.Features.Auth;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController(FarmAppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName && u.IsActive, ct);
        if (user is null) return Unauthorized();

        var hasher = new PasswordHasher<AppUser>();
        var check = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (check == PasswordVerificationResult.Failed) return Unauthorized();

        var refresh = new RefreshToken
        {
            AppUserId = user.AppUserId,
            Token = tokens.CreateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        db.RefreshTokens.Add(refresh);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(tokens.CreateAccessToken(user), refresh.Token);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var existing = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && t.RevokedAt == null, ct);
        if (existing is null || existing.ExpiresAt < DateTime.UtcNow) return Unauthorized();

        var user = await db.Users.FirstAsync(u => u.AppUserId == existing.AppUserId, ct);
        existing.RevokedAt = DateTime.UtcNow;

        var newRefresh = new RefreshToken
        {
            AppUserId = user.AppUserId,
            Token = tokens.CreateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        db.RefreshTokens.Add(newRefresh);
        await db.SaveChangesAsync(ct);

        return new TokenResponse(tokens.CreateAccessToken(user), newRefresh.Token);
    }
}
```

### 7.5 — Turn authentication on and make deny-by-default real

**VS:** open `Program.cs`. Add these `using`s and this block *before* `var app = builder.Build();`:
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FarmApp.Api.Features.Auth;

builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build())
    .AddPolicy("CanManageMasterData", p => p.RequireRole("Owner"));
```

Then, in the pipeline section (after `app.UseHttpsRedirection();`, before `app.UseAuthorization();`), add:
```csharp
app.UseAuthentication();
```
(`UseAuthentication` must run before `UseAuthorization` — it decides *who you are*; authorization then decides *what you're allowed to do*.)

Finally, put `[Authorize(Policy = "CanManageMasterData")]` above the `Create`, `Update`, and `Delete` methods in `GradesController.cs` — reads stay open to any authenticated user, writes require the Owner role.

### 7.6 — Test the lock in Swagger

In Swagger, log in via `POST /api/v1/auth/login` with `andri` / your seeded password, copy the `accessToken`, click the padlock icon at the top of the Swagger page, paste `Bearer <token>`. Then:
- No token → `GET /api/v1/grades` works (fallback policy just requires *any* authenticated user — wait, actually try it unauthenticated first): expect **401**.
- With a token that has role `Cashier` (create one via a quick temporary edit to the seeder, or via SSMS) on `POST /api/v1/grades`: expect **403**.
- With the Owner token: expect **200**.

Seeing 401 vs 403 side by side teaches the authentication-vs-authorization difference better than any article.

✅ **Checkpoint:** the three-way 401/403/200 test passes; restarting the API doesn't break an issued refresh token (because it's stored in the database, not in memory). Commit: `"M5 auth"`.

## Step 8 — Logging pipeline (M6) — spec: [doc 13](<../AI Guide/13-auth-and-logging.md>) Part B

*Concept:* middleware = a nesting-doll pipeline around every request. Log once in the pipeline → **zero logger calls in services/repos** (the hard rule).

### 8.1 — Install and configure Serilog

```powershell
dotnet add src\FarmApp.Api package Serilog.AspNetCore
```

**VS:** in `Program.cs`, add `using Serilog;` at the top, and this line as the very first statement in the file (before `var builder = ...`):
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/farmapp-.json", rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31, formatter: new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
```

### 8.2 — Correlation ID middleware

**VS:** right-click **FarmApp.Api** → Add → New Folder → `Middleware`. Add → Class → `CorrelationIdMiddleware.cs`:
```csharp
using Serilog.Context;

namespace FarmApp.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var correlationId = Guid.NewGuid().ToString();
        ctx.Items["CorrelationId"] = correlationId;
        ctx.Response.Headers["X-Correlation-Id"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(ctx);
        }
    }
}
```

### 8.3 — Request/response logging middleware

Add → Class (in `Middleware\`) → `RequestResponseLoggingMiddleware.cs`:
```csharp
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FarmApp.Api.Middleware;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    private static readonly string[] SkipPaths = ["/swagger", "/openapi", "/health"];

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (SkipPaths.Any(p => ctx.Request.Path.StartsWithSegments(p)))
        {
            await next(ctx);
            return;
        }

        ctx.Request.EnableBuffering();
        var reqBody = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
        ctx.Request.Body.Position = 0;

        var original = ctx.Response.Body;
        await using var buffer = new MemoryStream();
        ctx.Response.Body = buffer;

        var sw = Stopwatch.StartNew();
        await next(ctx);
        sw.Stop();

        buffer.Position = 0;
        var resBody = await new StreamReader(buffer).ReadToEndAsync();

        var level = ctx.Response.StatusCode >= 500 ? LogLevel.Error
            : ctx.Response.StatusCode >= 400 ? LogLevel.Warning
            : LogLevel.Information;

        logger.Log(level, "HTTP {Method} {Path} by {User} => {Status} in {Ms}ms | req {Req} | res {Res}",
            ctx.Request.Method, ctx.Request.Path, ctx.User.Identity?.Name ?? "anon",
            ctx.Response.StatusCode, sw.ElapsedMilliseconds,
            Redact(Truncate(reqBody)), Redact(Truncate(resBody)));

        buffer.Position = 0;
        await buffer.CopyToAsync(original);
    }

    private static string Truncate(string s) => s.Length > 8000 ? s[..8000] + "...[truncated]" : s;

    private static readonly Regex SensitiveField =
        new("\"(password|pin|refreshToken)\"\\s*:\\s*\"[^\"]*\"", RegexOptions.IgnoreCase);

    private static string Redact(string s) => SensitiveField.Replace(s, "\"$1\":\"***\"");
}
```

### 8.4 — Exception-handling middleware

Add → Class (in `Middleware\`) → `ExceptionHandlingMiddleware.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace FarmApp.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            var correlationId = ctx.Items["CorrelationId"]?.ToString() ?? "unknown";
            logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = 500,
                Detail = $"Reference: {correlationId}",
            };
            await ctx.Response.WriteAsJsonAsync(problem);
        }
    }
}
```

### 8.5 — Register the pipeline, in this exact order

**VS:** in `Program.cs`, place these three lines *before* `app.UseAuthentication();` (exception handler outermost, then correlation, then logging):
```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
```

### 8.6 — Test it

Run the API, make a normal `GET /api/v1/grades` call, a bad `POST` (missing name), and temporarily `throw new Exception("test")` inside one controller action to see the 500 path — then remove that line. Open `logs\farmapp-<date>.json` and find all three log entries; copy a correlation ID from a response's `X-Correlation-Id` header and find its matching log line.

✅ **Checkpoint:** every request produces exactly one structured log event; passwords show as `***`; grep-by-correlation-id works. Commit: `"M6 logging pipeline"`.

## Step 9 — From here on: repeat with intent (M7+)

The teaching phase is over; now it's reps. For **every new feature**, follow the *exact same recipe* as Grade in steps 2, 4, and 5 — same folder shapes, same "full file, exact path" discipline, just a new entity name:

```
entity (Domain/Entities) → configuration (Infrastructure/Persistence/Configurations)
→ migration (.\ef-add.ps1 <Name>) → repository interface (Domain/Repositories)
→ repository implementation (Infrastructure/Persistence/Repositories) → DI registration (Program.cs)
→ DTOs + validator + controller (Api/Features/<Name>) → policy → (FE screen, see frontend guide)
```

1. **Master data** (M7): Product (with ProductType/MakeMode — [doc 02](<../AI Guide/02-data-model.md>)), PackSize, Block, Crop/Cultivar, InputItem, Supplier, Customer, PriceList/Price. Also `AccountingPeriod` + its SaveChanges date-check interceptor, and the audit interceptor ([doc 10](<../AI Guide/10-go-live-controls.md>), [doc 12](<../AI Guide/12-implementation-handoff.md>)) — interceptors are middleware's cousin for the DB side.
2. **Stock** (Phase 1, [doc 06](<../AI Guide/06-roadmap.md>)): StockBatch + StockMovement. First *domain service* with real rules: `StockService` ([doc 11](<../AI Guide/11-coding-standards.md>) — logic in Domain, unit-testable). Write your first unit tests here: xUnit project, test FIFO depletion and "balance = SUM(movements)" with an in-memory list, no DB.
3. **POS** (Phase 2): Sale/SaleLine/SalePayment/TillSession + **ClientGuid idempotency** (check-exists-first → return existing; unique index as backstop) — [doc 08](<../AI Guide/08-offline-sync.md>).
4. **Farming** (Phase 3), **Costing + reports** (Phase 4 — Dapper over your Reporting views, SQL guide step 5), per [doc 06](<../AI Guide/06-roadmap.md>).

Rhythm: one vertical slice per sitting — entity to screen. Small, finished, committed.
