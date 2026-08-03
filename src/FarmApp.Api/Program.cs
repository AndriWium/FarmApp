using FarmApp.Domain.Repositories;
using FarmApp.Infrastructure.Persistence;
using FarmApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FarmAppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("FarmApp")));

builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FarmAppDbContext>());

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
