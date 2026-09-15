using System.Text;
using FarmApp.Api.Application.Auth;
using FarmApp.Api.Application.Blocks;
using FarmApp.Api.Application.Crops;
using FarmApp.Api.Application.Cultivars;
using FarmApp.Api.Application.Grades;
using FarmApp.Api.Application.InputItems;
using FarmApp.Api.Middleware;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Infrastructure.Persistence;
using FarmApp.Infrastructure.Persistence.Interceptors;
using FarmApp.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/farmapp-.json",
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddDbContext<FarmAppDbContext>((sp, o) =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("FarmApp"));
    // PeriodLockInterceptor has no dependencies (inert until an IPeriodLocked entity exists —
    // doc 10 §1); AuditInterceptor needs the scoped IHttpContextAccessor, so it's resolved from
    // the request's own service provider rather than newed up directly.
    o.AddInterceptors(new PeriodLockInterceptor(), sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<ICultivarRepository, CultivarRepository>();
builder.Services.AddScoped<IInputItemRepository, InputItemRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FarmAppDbContext>());

builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<ICultivarService, CultivarService>();
builder.Services.AddScoped<IInputItemService, InputItemService>();

builder.Services.AddScoped<IValidator<CreateGradeRequest>, CreateGradeRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateGradeRequest>, UpdateGradeRequestValidator>();
builder.Services.AddScoped<IValidator<CreateBlockRequest>, CreateBlockRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateBlockRequest>, UpdateBlockRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCropRequest>, CreateCropRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCropRequest>, UpdateCropRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCultivarRequest>, CreateCultivarRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCultivarRequest>, UpdateCultivarRequestValidator>();
builder.Services.AddScoped<IValidator<CreateInputItemRequest>, CreateInputItemRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateInputItemRequest>, UpdateInputItemRequestValidator>();

builder.Services.AddScoped<TokenService>();

builder.Services.AddCors(o => o.AddPolicy("Fe", p => p
    .WithOrigins("http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()));

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

builder.Services.AddControllers()
    // InputItem.Category is the first enum exposed through the API; serialize enums as
    // strings everywhere (matches the "stored as strings for report readability" DB rule -
    // doc 11) rather than the default numeric JSON representation.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddProblemDetails();

// Swashbuckle's own SwaggerGen document generation (replaces Microsoft.AspNetCore.OpenApi's
// AddOpenApi/MapOpenApi — see DECISIONS.md) so the standard AddSecurityDefinition/
// AddSecurityRequirement bearer-auth padlock wiring works against a stable, documented API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "FarmApp API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer",
    });
    o.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() },
    });
});

var app = builder.Build();

// Idempotent seed: first Owner user, so the app runs end to end out of the box.
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmApp API"));
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseCors("Fe");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
