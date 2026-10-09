using MaliMove.Api.DTOs;
using MaliMove.Api.Interfaces;

namespace MaliMove.Api.Services;

public class ProductService : IProductService
{
    private readonly IMockDataRepository _repo;

    public ProductService(IMockDataRepository repo) => _repo = repo;

    public async Task<List<ProductSearchResultDto>> SearchProductsAsync(string query)
    {
        var products = await _repo.GetProductsAsync();
        var prices = await _repo.GetPricesAsync();
        var stores = await _repo.GetStoresAsync();
        var retailers = await _repo.GetRetailersAsync();
        var deals = await _repo.GetDealsAsync();

        var storeMap = stores.ToDictionary(s => s.Id);
        var retailerMap = retailers.ToDictionary(r => r.Id);
        var activeDealMap = deals.Where(d => d.IsActive)
            .ToDictionary(d => $"{d.ProductId}_{d.StoreId}");

        var lower = query.ToLowerInvariant();
        var matched = products.Where(p =>
            p.Name.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
            p.Category.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
            p.Brand.Contains(lower, StringComparison.OrdinalIgnoreCase));

        return matched.Select(p =>
        {
            var productPrices = prices.Where(pr => pr.ProductId == p.Id).ToList();
            var storePrices = productPrices.Select(pp =>
            {
                storeMap.TryGetValue(pp.StoreId, out var store);
                var retailerName = store != null && retailerMap.TryGetValue(store.RetailerId, out var r) ? r.Name : pp.StoreId;
                activeDealMap.TryGetValue($"{p.Id}_{pp.StoreId}", out var deal);
                return new StorePriceDto(pp.StoreId, store?.Name ?? pp.StoreId, retailerName,
                    pp.Price, deal != null, deal?.DealPrice);
            }).OrderBy(sp => sp.HasDeal ? sp.DealPrice ?? sp.Price : sp.Price).ToList();

            var cheapest = storePrices.FirstOrDefault();
            return new ProductSearchResultDto(p.Id, p.Name, p.Category, p.Brand, p.Unit,
                cheapest?.Price, cheapest?.StoreId, cheapest?.StoreName, storePrices, p.Alternatives);
        }).ToList();
    }

    public async Task<ProductSearchResultDto?> GetProductByIdAsync(string id)
    {
        var results = await SearchProductsAsync(id);
        return results.FirstOrDefault(r => r.Id == id);
    }

    public async Task<List<BrandAlternativeDto>> GetBrandAlternativesAsync(string productId)
    {
        var products = await _repo.GetProductsAsync();
        var prices = await _repo.GetPricesAsync();
        var stores = await _repo.GetStoresAsync();
        var retailers = await _repo.GetRetailersAsync();

        var storeMap = stores.ToDictionary(s => s.Id);
        var retailerMap = retailers.ToDictionary(r => r.Id);

        var source = products.FirstOrDefault(p => p.Id == productId);
        if (source == null) return new();

        var alternativeIds = source.Alternatives.Concat(new[] { productId }).Distinct();
        var alternativeProducts = products.Where(p => alternativeIds.Contains(p.Id)).ToList();

        var sourceLowestPrice = prices.Where(p => p.ProductId == productId)
            .Select(p => p.Price).DefaultIfEmpty(0).Min();

        return alternativeProducts.SelectMany(p =>
        {
            var productPrices = prices.Where(pr => pr.ProductId == p.Id).ToList();
            if (!productPrices.Any()) return Enumerable.Empty<BrandAlternativeDto>();
            var cheapest = productPrices.OrderBy(pp => pp.Price).First();
            storeMap.TryGetValue(cheapest.StoreId, out var store);
            var retailerName = store != null && retailerMap.TryGetValue(store.RetailerId, out var r) ? r.Name : cheapest.StoreId;
            return new[] { new BrandAlternativeDto(p.Id, p.Name, p.Brand, cheapest.Price,
                cheapest.StoreId, store?.Name ?? cheapest.StoreId,
                sourceLowestPrice - cheapest.Price,
                p.Brand.Contains("Fresh Smile") || p.Brand.Contains("Clean") || p.Brand.Contains("No Name") || p.Brand.Contains("Farmfresh")) };
        }).OrderBy(a => a.Price).ToList();
    }
}
