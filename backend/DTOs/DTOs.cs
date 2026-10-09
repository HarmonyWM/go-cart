namespace MaliMove.Api.DTOs;

public record StoreDto(
    string Id, string RetailerId, string RetailerName, string Name,
    string Address, double DistanceKm, int EstimatedShoppingMinutes, bool IsLocal, string Color);

public record ProductDto(
    string Id, string Name, string Category, string Brand,
    string Unit, string? ImageUrl, List<string> Alternatives);

public record ProductSearchResultDto(
    string Id, string Name, string Category, string Brand, string Unit,
    decimal? LowestPrice, string? LowestPriceStoreId, string? LowestPriceStoreName,
    List<StorePriceDto> StorePrices, List<string> Alternatives);

public record StorePriceDto(string StoreId, string StoreName, string RetailerName, decimal Price, bool HasDeal, decimal? DealPrice);

public record DealDto(
    string Id, string ProductId, string ProductName, string StoreId, string StoreName,
    string RetailerId, string RetailerName, string Title, string Description,
    decimal DealPrice, decimal OriginalPrice, int DiscountPercent, decimal SavingsAmount,
    DateTime ValidUntil, string Category, bool IsActive);

public record ShoppingListItemDto(string ProductId, string ProductName, int Quantity, bool IsPurchased);

public record ShoppingListDto(
    string Id, string Name, List<ShoppingListItemDto> Items,
    decimal? Budget, string Priority, DateTime CreatedAt);

public record CreateShoppingListRequest(
    string Name, List<ShoppingListItemDto> Items, decimal? Budget, string Priority);

public record OptimiseRequest(
    List<ShoppingListItemDto> Items,
    decimal? Budget,
    string Priority,
    UserPreferencesDto Preferences);

public record UserPreferencesDto(
    List<string> PreferredStoreIds,
    double MaxTravelDistanceKm,
    string TransportMode,
    string FuelType,
    double VehicleConsumptionLPer100km,
    decimal FuelPricePerLitre,
    bool WillingToSwitchBrands,
    string DefaultPriority);

public record OptimisationResultDto(
    List<ShoppingOptionDto> Options,
    ShoppingOptionDto Recommended,
    string RecommendationReason,
    decimal? Budget,
    string BudgetStatus);

public record ShoppingOptionDto(
    string Key,
    string Label,
    string Icon,
    List<StoreBasketDto> StoreBaskets,
    decimal ProductTotal,
    decimal TravelCost,
    decimal DeliveryCost,
    decimal TotalEstimatedCost,
    int StoreCount,
    decimal EstimatedSavings,
    int EstimatedTotalMinutes,
    double TotalDistanceKm,
    string Description,
    bool IsRecommended);

public record StoreBasketDto(
    string StoreId, string StoreName, string RetailerName, string Color,
    List<BasketItemDto> Items, decimal SubTotal, double DistanceKm, decimal TravelCost);

public record BasketItemDto(
    string ProductId, string ProductName, string Brand, int Quantity,
    decimal UnitPrice, decimal LineTotal, bool HasDeal, decimal? OriginalPrice);

public record BrandAlternativeDto(
    string ProductId, string ProductName, string Brand, decimal Price,
    string StoreId, string StoreName, decimal SavingsVsSelected, bool IsGoCartSuggestion);
