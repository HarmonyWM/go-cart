using MaliMove.Api.DTOs;
using MaliMove.Api.Interfaces;

namespace MaliMove.Api.Services;

public class StoreService : IStoreService
{
    private readonly IMockDataRepository _repo;

    public StoreService(IMockDataRepository repo) => _repo = repo;

    public async Task<List<StoreDto>> GetAllStoresAsync()
    {
        var stores = await _repo.GetStoresAsync();
        var retailers = await _repo.GetRetailersAsync();
        var retailerMap = retailers.ToDictionary(r => r.Id);

        return stores.Select(s =>
        {
            retailerMap.TryGetValue(s.RetailerId, out var retailer);
            return new StoreDto(s.Id, s.RetailerId, retailer?.Name ?? s.RetailerId,
                s.Name, s.Address, s.DistanceKm, s.EstimatedShoppingMinutes,
                retailer?.IsLocal ?? false, retailer?.Color ?? "#333");
        }).ToList();
    }

    public async Task<StoreDto?> GetStoreByIdAsync(string id)
    {
        var all = await GetAllStoresAsync();
        return all.FirstOrDefault(s => s.Id == id);
    }
}
