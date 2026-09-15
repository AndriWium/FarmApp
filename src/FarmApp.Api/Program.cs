using System.Text;
using FarmApp.Api.Application.Auth;
using FarmApp.Api.Application.Blocks;
using FarmApp.Api.Application.Crops;
using FarmApp.Api.Application.Grades;
using FarmApp.Domain.Entities;
using FarmApp.Domain.Repositories;
using FarmApp.Infrastructure.Persistence;
using FarmApp.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FarmAppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("FarmApp")));

builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FarmAppDbContext>());

builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IBlockService, BlockService>();
builder.Services.AddScoped<ICropService, CropService>();

builder.Services.AddScoped<IValidator<CreateGradeRequest>, CreateGradeRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateGradeRequest>, UpdateGradeRequestValidator>();
builder.Services.AddScoped<IValidator<CreateBlockRequest>, CreateBlockRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateBlockRequest>, UpdateBlockRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCropRequest>, CreateCropRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCropRequest>, UpdateCropRequestValidator>();

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

builder.Services.AddControllers();

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

app.UseCors("Fe");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
