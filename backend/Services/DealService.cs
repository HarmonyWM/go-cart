using MaliMove.Api.DTOs;
using MaliMove.Api.Interfaces;

namespace MaliMove.Api.Services;

public class DealService : IDealService
{
    private readonly IMockDataRepository _repo;

    public DealService(IMockDataRepository repo) => _repo = repo;

    public async Task<List<DealDto>> GetAllDealsAsync(string? storeId = null, string? category = null, string? search = null)
    {
        var deals = await _repo.GetDealsAsync();
        var products = await _repo.GetProductsAsync();
        var stores = await _repo.GetStoresAsync();
        var retailers = await _repo.GetRetailersAsync();

        var productMap = products.ToDictionary(p => p.Id);
        var storeMap = stores.ToDictionary(s => s.Id);
        var retailerMap = retailers.ToDictionary(r => r.Id);

        var active = deals.Where(d => d.IsActive);
        if (!string.IsNullOrEmpty(storeId)) active = active.Where(d => d.StoreId == storeId);
        if (!string.IsNullOrEmpty(category)) active = active.Where(d => d.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(search)) active = active.Where(d => d.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

        return active.Select(d =>
        {
            productMap.TryGetValue(d.ProductId, out var product);
            storeMap.TryGetValue(d.StoreId, out var store);
            retailerMap.TryGetValue(d.RetailerId, out var retailer);
            return new DealDto(d.Id, d.ProductId, product?.Name ?? d.ProductId,
                d.StoreId, store?.Name ?? d.StoreId, d.RetailerId, retailer?.Name ?? d.RetailerId,
                d.Title, d.Description, d.DealPrice, d.OriginalPrice, d.DiscountPercent,
                d.SavingsAmount, d.ValidUntil, d.Category, d.IsActive);
        }).ToList();
    }

    public async Task<List<DealDto>> GetDealsMatchingListAsync(List<string> productIds)
    {
        var all = await GetAllDealsAsync();
        return all.Where(d => productIds.Contains(d.ProductId)).ToList();
    }
}
