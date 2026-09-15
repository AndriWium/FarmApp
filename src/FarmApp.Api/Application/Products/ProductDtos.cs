using FarmApp.Domain.Enums;

namespace FarmApp.Api.Application.Products;

public record ProductDto(
    int ProductId,
    string Name,
    ProductType ProductType,
    int? CropId,
    MakeMode? MakeMode,
    ProductBaseUnit BaseUnit,
    bool IsActive
    );

public record CreateProductRequest(
    string Name,
    ProductType ProductType,
    int? CropId,
    MakeMode? MakeMode,
    ProductBaseUnit BaseUnit
    );

public record UpdateProductRequest(
    string Name,
    ProductType ProductType,
    int? CropId,
    MakeMode? MakeMode,
    ProductBaseUnit BaseUnit,
    bool IsActive
    );
