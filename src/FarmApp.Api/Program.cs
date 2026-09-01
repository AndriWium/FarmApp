using FarmApp.Api.Features.Blocks;
using FarmApp.Api.Features.Crops;
using FarmApp.Api.Features.Grades;
using FarmApp.Domain.Repositories;
using FarmApp.Infrastructure.Persistence;
using FarmApp.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IValidator<CreateBlockRequest>, CreateBlockRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCropRequest>, CreateCropRequestValidator>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "FarmApp API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
