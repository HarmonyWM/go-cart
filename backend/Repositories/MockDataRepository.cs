using System.Text.Json;
using GoCart.Api.Interfaces;
using GoCart.Api.Models;

namespace GoCart.Api.Repositories;

public class MockDataRepository : IMockDataRepository
{
    private readonly string _dataPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MockDataRepository(IConfiguration config)
    {
        _dataPath = config["MockDataPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "mock-data");
    }

    private async Task<List<T>> LoadAsync<T>(string fileName)
    {
        var path = Path.Combine(_dataPath, fileName);
        if (!File.Exists(path)) return new List<T>();
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }

    public Task<List<Retailer>> GetRetailersAsync() => LoadAsync<Retailer>("retailers.json");
    public Task<List<Store>> GetStoresAsync() => LoadAsync<Store>("stores.json");
    public Task<List<Product>> GetProductsAsync() => LoadAsync<Product>("products.json");
    public Task<List<ProductPrice>> GetPricesAsync() => LoadAsync<ProductPrice>("prices.json");
    public Task<List<Deal>> GetDealsAsync() => LoadAsync<Deal>("deals.json");
}
