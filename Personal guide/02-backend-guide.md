# Backend Guide — Step by Step (.NET 9, EF Core code-first)

Builds milestones M2, M3, M5, M6 from [00-how-to-use-this-guide.md](00-how-to-use-this-guide.md). Each step teaches one pattern; from M7 you repeat them. Type everything yourself.

---

## Step 1 — Solution skeleton (M2 begins)

*Concept in one line:* the three projects enforce the [doc 11](<../AI Guide/11-coding-standards.md>) dependency rule mechanically — Domain can't secretly use EF because it literally has no reference to it.

```powershell
cd H:\FarmApp
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

Also now: `dotnet new gitignore`, and an empty `LEARNING.md` + `DECISIONS.md` (seed it from [AI Guide README](<../AI Guide/README.md>)'s decision list). Commit.

✅ **Checkpoint:** `dotnet build` says Build succeeded, 0 warnings worth worrying about. Commit: `"M2.1 solution skeleton"`.

## Step 2 — First entity + DbContext

*Concept:* an **entity** is a plain C# class EF maps to a table. The **DbContext** is the gateway: one `DbSet<T>` per table, plus configuration. Start with the smallest table in [doc 02](<../AI Guide/02-data-model.md>) — `Grade` — because the pattern matters, not the table.

1. In **Domain**, folder `Entities\`, class `Grade` with `int GradeId` and `string Name` (init `= null!;`).
2. In **Infrastructure**, install EF:
```powershell
dotnet add src\FarmApp.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src\FarmApp.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src\FarmApp.Api package Microsoft.EntityFrameworkCore.Design
```
3. In Infrastructure, `Persistence\FarmAppDbContext.cs`:
```csharp
public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options) : DbContext(options)
{
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder mb)
        => mb.ApplyConfigurationsFromAssembly(typeof(FarmAppDbContext).Assembly);
}
```
4. Configuration class (the [doc 11](<../AI Guide/11-coding-standards.md>) rule: fluent config, not attributes) in `Persistence\Configurations\GradeConfiguration.cs`:
```csharp
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
5. In **Api** `appsettings.Development.json`, add the connection string from [doc 07](<../AI Guide/07-database-setup.md>). In `Program.cs`:
```csharp
builder.Services.AddDbContext<FarmAppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("FarmApp")));
```

✅ **Checkpoint:** builds. You can explain: entity = row shape, DbSet = table handle, configuration = column rules.

## Step 3 — First migration (M2 done)

*Concept:* a **migration** is a generated C# file describing schema changes; `database update` runs it as SQL. Your schema history becomes source-controlled files — that's the whole magic of code-first.

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src\FarmApp.Infrastructure --startup-project src\FarmApp.Api
dotnet ef database update             --project src\FarmApp.Infrastructure --startup-project src\FarmApp.Api
```

Open the generated file under `Migrations\` and *read it* — see your `HasMaxLength(50)` become `maxLength: 50`. Then SSMS: refresh Databases → `FarmApp` exists → Grade + `__EFMigrationsHistory` tables (SQL guide step 4).

Those two long commands get old fast — put them in `ef-add.ps1` / `ef-update.ps1` scripts now.

✅ **Checkpoint:** FarmApp DB visible in SSMS with the Grade table shaped exactly as configured. Commit: `"M2.3 initial migration"`.

## Step 4 — Repository + Unit of Work (the pattern you'll reuse forever)

*Concept:* the repository hides EF behind an interface **owned by Domain** (dependency inversion — the D in SOLID). Services say *what* they need; Infrastructure knows *how*.

1. **Domain** `Repositories\IGradeRepository.cs`:
```csharp
public interface IGradeRepository
{
    Task<Grade?> GetByIdAsync(int id, CancellationToken ct);
    Task<List<Grade>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Grade grade, CancellationToken ct);
    void Remove(Grade grade);
}
public interface IUnitOfWork { Task<int> SaveChangesAsync(CancellationToken ct); }
```
2. **Infrastructure** implements: `GradeRepository` wrapping the DbContext (`_db.Grades.AsNoTracking().ToListAsync(ct)` for reads — [doc 11](<../AI Guide/11-coding-standards.md>) rule), and make `FarmAppDbContext` implement `IUnitOfWork` (it already has SaveChangesAsync).
3. **Api** `Program.cs` DI registrations:
```csharp
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FarmAppDbContext>());
```

*Why bother when EF exists?* Testability (fake `IGradeRepository`, no DB needed) and one place per aggregate where queries live. And per [doc 11](<../AI Guide/11-coding-standards.md>): reports will *bypass* repositories on purpose — repositories are for transactional work.

✅ **Checkpoint:** builds; you can explain aloud why Domain compiles without EF installed.

## Step 5 — First controller: Grades CRUD (M3)

*Concept:* controller = HTTP translator. DTOs in, DTOs out; entities never cross the wire ([doc 11](<../AI Guide/11-coding-standards.md>)).

1. In Api, `Features\Grades\`: `GradeDto(int GradeId, string Name)`, `CreateGradeRequest(string Name)` — C# records.
2. `GradesController` with `[ApiController] [Route("api/v1/[controller]")]`: GET all, GET by id (404 when null), POST (create → 201), PUT, DELETE. Constructor-inject `IGradeRepository` + `IUnitOfWork`; every write ends with `await _uow.SaveChangesAsync(ct);`.
3. Run `dotnet run --project src\FarmApp.Api` → open the Swagger URL from the console output. Exercise every verb from Swagger: create "Class 1", list it, rename it, delete it. Watch rows appear in SSMS as you go.
4. Add FluentValidation (`dotnet add src\FarmApp.Api package FluentValidation.AspNetCore`): rule `Name NotEmpty, MaxLength 50` → verify a bad POST returns 400 with a field message, not a 500.

✅ **Checkpoint:** full CRUD via Swagger, validation errors are clean 400s. Commit: `"M3 grades CRUD"`. **This step is the template for every master-data table in the app.**

## Step 6 — Pause: wire the frontend (M4)

Jump to [03-frontend-guide.md](03-frontend-guide.md) steps 1–5 and get Angular listing your grades. Reason: seeing the full loop FE → API → DB early changes how you think about everything after; and you'll hit CORS now, with the simplest possible setup to debug it in (the FE guide covers the fix).

## Step 7 — Authentication & authorization (M5) — spec: [doc 13](<../AI Guide/13-auth-and-logging.md>) Part A

Order matters here; each sub-step is testable:

1. **Entities + migration:** `AppUser` (UserId, UserName unique, PasswordHash, Role string, IsActive) and `RefreshToken` (Token, UserId, ExpiresAt, RevokedAt NULL). Migrate.
2. **Password hashing:** `dotnet add src\FarmApp.Api package Microsoft.Extensions.Identity.Core` → use `PasswordHasher<AppUser>` (`HashPassword`, `VerifyHashedPassword`). Never roll your own — this one does salting + safe comparison for you.
3. **Seed the first Owner** user in an idempotent seeder called at startup (username `andri`, password from config, *hash it*).
4. **Login endpoint:** `POST /api/v1/auth/login` → verify password → issue JWT. Packages: `Microsoft.AspNetCore.Authentication.JwtBearer`. JWT settings (issuer, audience, signing key ≥ 32 chars) in user-secrets, not in the repo: `dotnet user-secrets init` + `set`. Claims: user id, name, role. Expiry ~15 min.
5. **Turn authentication on** (`AddAuthentication().AddJwtBearer(...)`, `UseAuthentication()` before `UseAuthorization()`), and make **deny-by-default** real:
```csharp
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().Build())
    .AddPolicy("CanManageMasterData", p => p.RequireRole("Owner"));
```
   Mark only login/refresh `[AllowAnonymous]`. Put `[Authorize(Policy = "CanManageMasterData")]` on Grades writes. Policy names per the [doc 13](<../AI Guide/13-auth-and-logging.md>) matrix, mapped in this one place.
6. **Test the lock** in Swagger (add the padlock config so you can paste a token): no token → 401; Cashier token on a master-data write → 403; Owner token → 200. Seeing 401 vs 403 teaches the authN/authZ difference better than any article.
7. **Refresh endpoint:** login also returns a refresh token (random 64-byte string, stored hashed-or-plain in the table with expiry); `POST /api/v1/auth/refresh` validates it, **revokes it, issues a new pair** (rotation, per [doc 13](<../AI Guide/13-auth-and-logging.md>)).

✅ **Checkpoint:** the three-way 401/403/200 test passes; restarting the API doesn't break an issued refresh token. Commit: `"M5 auth"`.

## Step 8 — Logging pipeline (M6) — spec: [doc 13](<../AI Guide/13-auth-and-logging.md>) Part B

*Concept:* middleware = a nesting-doll pipeline around every request. Log once in the pipeline → **zero logger calls in services/repos** (the hard rule).

1. `dotnet add src\FarmApp.Api package Serilog.AspNetCore` → in Program.cs: `builder.Host.UseSerilog(...)` writing console + rolling JSON file `logs\farmapp-.json` (retain ~31 days).
2. **CorrelationIdMiddleware** (~15 lines): new `Guid` per request → into `HttpContext.Items`, Serilog `LogContext.PushProperty`, and an `X-Correlation-Id` response header.
3. **RequestResponseLoggingMiddleware** — the interesting one. Skeleton of the body-capture trick:
```csharp
public async Task InvokeAsync(HttpContext ctx)
{
    ctx.Request.EnableBuffering();                       // allow re-reading request body
    var reqBody = await ReadAsync(ctx.Request);          // read, then rewind position to 0

    var original = ctx.Response.Body;                    // swap response stream
    await using var buffer = new MemoryStream();
    ctx.Response.Body = buffer;

    var sw = Stopwatch.StartNew();
    await _next(ctx);                                    // run the REST of the pipeline
    sw.Stop();

    buffer.Position = 0;
    var resBody = await new StreamReader(buffer).ReadToEndAsync();
    _logger.LogInformation("HTTP {Method} {Path} by {User} => {Status} in {Ms}ms | req {Req} | res {Res}",
        ctx.Request.Method, ctx.Request.Path, ctx.User.Identity?.Name ?? "anon",
        ctx.Response.StatusCode, sw.ElapsedMilliseconds,
        Redact(Truncate(reqBody)), Redact(Truncate(resBody)));

    buffer.Position = 0;
    await buffer.CopyToAsync(original);                  // hand the real body back
}
```
   Add the [doc 13](<../AI Guide/13-auth-and-logging.md>) guardrails: `Truncate` at 8 KB; `Redact` replaces values of `password/pin/refreshToken`; skip swagger/health/static paths; level Warning for 4xx, Error for 5xx.
4. **ExceptionHandlingMiddleware** (outermost): catch → log with correlation id → return 500 `ProblemDetails` whose detail is just the correlation id — the user sees a reference code, the log has the truth.
5. Test: make good calls, a validation failure, and a thrown exception → find all three in the log file; grab a correlation id from a response header and find its line.

✅ **Checkpoint:** every request = exactly one structured log event; passwords show as `***`; grep-by-correlation-id works. Commit: `"M6 logging pipeline"`.

## Step 9 — From here on: repeat with intent (M7+)

The teaching phase is over; now it's reps. For **every new feature**: entity → configuration → migration → repository interface → implementation → DI → DTOs → validator → controller → policy → (FE screen). In this order:

1. **Master data** (M7): Product (with ProductType/MakeMode — [doc 02](<../AI Guide/02-data-model.md>)), PackSize, Block, Crop/Cultivar, InputItem, Supplier, Customer, PriceList/Price. Also `AccountingPeriod` + its SaveChanges date-check interceptor, and the audit interceptor ([doc 10](<../AI Guide/10-go-live-controls.md>), [doc 12](<../AI Guide/12-implementation-handoff.md>)) — interceptors are middleware's cousin for the DB side.
2. **Stock** (Phase 1, [doc 06](<../AI Guide/06-roadmap.md>)): StockBatch + StockMovement. First *domain service* with real rules: `StockService` ([doc 11](<../AI Guide/11-coding-standards.md>) — logic in Domain, unit-testable). Write your first unit tests here: xUnit project, test FIFO depletion and "balance = SUM(movements)" with an in-memory list, no DB.
3. **POS** (Phase 2): Sale/SaleLine/SalePayment/TillSession + **ClientGuid idempotency** (check-exists-first → return existing; unique index as backstop) — [doc 08](<../AI Guide/08-offline-sync.md>).
4. **Farming** (Phase 3), **Costing + reports** (Phase 4 — Dapper over your Reporting views, SQL guide step 5), per [doc 06](<../AI Guide/06-roadmap.md>).

Rhythm: one vertical slice per sitting — entity to screen. Small, finished, committed.
