using MaliMove.Api.DTOs;
using MaliMove.Api.Models;

namespace MaliMove.Api.Interfaces;

public interface IMockDataRepository
{
    Task<List<Retailer>> GetRetailersAsync();
    Task<List<Store>> GetStoresAsync();
    Task<List<Product>> GetProductsAsync();
    Task<List<ProductPrice>> GetPricesAsync();
    Task<List<Deal>> GetDealsAsync();
}

public interface IProductService
{
    Task<List<ProductSearchResultDto>> SearchProductsAsync(string query);
    Task<ProductSearchResultDto?> GetProductByIdAsync(string id);
    Task<List<BrandAlternativeDto>> GetBrandAlternativesAsync(string productId);
}

public interface IStoreService
{
    Task<List<StoreDto>> GetAllStoresAsync();
    Task<StoreDto?> GetStoreByIdAsync(string id);
}

public interface IDealService
{
    Task<List<DealDto>> GetAllDealsAsync(string? storeId = null, string? category = null, string? search = null);
    Task<List<DealDto>> GetDealsMatchingListAsync(List<string> productIds);
}

public interface IShoppingOptimisationService
{
    Task<OptimisationResultDto> OptimiseAsync(OptimiseRequest request);
}

public interface ITravelCostService
{
    decimal CalculateFuelCost(double distanceKm, double consumptionLPer100km, decimal fuelPricePerLitre);
    int EstimateTravelMinutes(double distanceKm, string transportMode);
}
