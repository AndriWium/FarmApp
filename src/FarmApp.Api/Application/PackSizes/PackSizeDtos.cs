namespace FarmApp.Api.Application.PackSizes;

public record PackSizeDto(int PackSizeId, int ProductId, string Name, decimal QtyInBaseUnit);

public record CreatePackSizeRequest(int ProductId, string Name, decimal QtyInBaseUnit);

public record UpdatePackSizeRequest(int ProductId, string Name, decimal QtyInBaseUnit);
