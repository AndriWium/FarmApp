namespace FarmApp.Api.Application.PriceLists;

public record PriceListDto(int PriceListId, string Name, bool IsActive);

public record CreatePriceListRequest(string Name);

public record UpdatePriceListRequest(string Name, bool IsActive);
