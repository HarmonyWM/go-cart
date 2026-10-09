using GoCart.Api.DTOs;
using GoCart.Api.Interfaces;
using GoCart.Api.Models;

namespace GoCart.Api.Services;

public class ShoppingOptimisationService : IShoppingOptimisationService
{
    private readonly IMockDataRepository _repo;
    private readonly ITravelCostService _travelCost;

    public ShoppingOptimisationService(IMockDataRepository repo, ITravelCostService travelCost)
    {
        _repo = repo;
        _travelCost = travelCost;
    }

    public async Task<OptimisationResultDto> OptimiseAsync(OptimiseRequest request)
    {
        var stores = await _repo.GetStoresAsync();
        var retailers = await _repo.GetRetailersAsync();
        var products = await _repo.GetProductsAsync();
        var prices = await _repo.GetPricesAsync();
        var deals = await _repo.GetDealsAsync();

        var retailerMap = retailers.ToDictionary(r => r.Id);
        var productMap = products.ToDictionary(p => p.Id);
        var activeDealMap = deals.Where(d => d.IsActive)
            .ToDictionary(d => $"{d.ProductId}_{d.StoreId}");

        var prefs = request.Preferences;

        // Build effective price lookup: productId+storeId -> best price (deal or regular)
        var effectivePrices = prices.ToDictionary(
            p => $"{p.ProductId}_{p.StoreId}",
            p =>
            {
                activeDealMap.TryGetValue($"{p.ProductId}_{p.StoreId}", out var deal);
                return (price: deal?.DealPrice ?? p.Price, hasDeal: deal != null, originalPrice: p.Price);
            });

        // Generate shopping options
        var options = new List<ShoppingOptionDto>();

        // Option 1: Single best store per priority
        var singleStoreOptions = GenerateSingleStoreOptions(stores, retailers, productMap,
            effectivePrices, request.Items, prefs, retailerMap);
        options.AddRange(singleStoreOptions);

        // Option 2: Multi-store cheapest basket
        var multiStoreOption = GenerateMultiStoreOption(stores, retailers, productMap,
            effectivePrices, request.Items, prefs, retailerMap);
        if (multiStoreOption != null) options.Add(multiStoreOption);

        if (!options.Any()) return new OptimisationResultDto(options, null!, "No options available", request.Budget, "unknown");

        // Find baseline (cheapest single store total)
        var baseline = options.Where(o => o.StoreCount == 1).OrderBy(o => o.TotalEstimatedCost).FirstOrDefault();
        var baselineTotal = baseline?.TotalEstimatedCost ?? options.Min(o => o.TotalEstimatedCost);

        // Add savings relative to baseline
        options = options.Select(o => o with
        {
            EstimatedSavings = Math.Max(0, baselineTotal - o.TotalEstimatedCost)
        }).ToList();

        var recommended = SelectRecommendation(options, request.Priority, request.Budget, prefs);
        var reason = BuildRecommendationReason(recommended, options, request.Priority, request.Budget);

        var budgetStatus = request.Budget.HasValue
            ? GetBudgetStatus(recommended.TotalEstimatedCost, request.Budget.Value)
            : "no_budget";

        return new OptimisationResultDto(options, recommended with { IsRecommended = true }, reason, request.Budget, budgetStatus);
    }

    private List<ShoppingOptionDto> GenerateSingleStoreOptions(
        List<Store> stores, List<Retailer> retailers, Dictionary<string, Product> productMap,
        Dictionary<string, (decimal price, bool hasDeal, decimal originalPrice)> effectivePrices,
        List<ShoppingListItemDto> items, UserPreferencesDto prefs, Dictionary<string, Retailer> retailerMap)
    {
        return stores
            .Where(s => s.DistanceKm <= prefs.MaxTravelDistanceKm)
            .Select(store =>
            {
                retailerMap.TryGetValue(store.RetailerId, out var retailer);
                var basketItems = BuildBasketItems(store.Id, items, productMap, effectivePrices);
                if (!basketItems.Any()) return null;

                var productTotal = basketItems.Sum(i => i.LineTotal);
                var travelCost = prefs.TransportMode == "driving"
                    ? _travelCost.CalculateFuelCost(store.DistanceKm, prefs.VehicleConsumptionLPer100km, prefs.FuelPricePerLitre)
                    : prefs.TransportMode == "publicTransport" ? (decimal)(store.DistanceKm * 2 * 2.50) : 0m;
                var travelMinutes = _travelCost.EstimateTravelMinutes(store.DistanceKm, prefs.TransportMode);

                return (ShoppingOptionDto?)new ShoppingOptionDto(
                    $"single_{store.Id}", store.Name, "🏪",
                    new List<StoreBasketDto> { new(store.Id, store.Name, retailer?.Name ?? store.RetailerId,
                        retailer?.Color ?? "#333", basketItems, productTotal, store.DistanceKm, travelCost) },
                    productTotal, travelCost, 0m, productTotal + travelCost, 1, 0m,
                    travelMinutes + store.EstimatedShoppingMinutes, store.DistanceKm,
                    $"Shop everything at {store.Name}", false);
            })
            .Where(o => o != null)
            .Select(o => o!.Value)
            .ToList();
    }

    private ShoppingOptionDto? GenerateMultiStoreOption(
        List<Store> stores, List<Retailer> retailers, Dictionary<string, Product> productMap,
        Dictionary<string, (decimal price, bool hasDeal, decimal originalPrice)> effectivePrices,
        List<ShoppingListItemDto> items, UserPreferencesDto prefs, Dictionary<string, Retailer> retailerMap)
    {
        // Assign each item to the cheapest store that stocks it
        var storeBaskets = new Dictionary<string, List<BasketItemDto>>();
        var storeObjects = stores.Where(s => s.DistanceKm <= prefs.MaxTravelDistanceKm)
            .ToDictionary(s => s.Id);

        foreach (var item in items)
        {
            var cheapestStore = storeObjects.Values
                .Where(s => effectivePrices.ContainsKey($"{item.ProductId}_{s.Id}"))
                .OrderBy(s => effectivePrices[$"{item.ProductId}_{s.Id}"].price)
                .FirstOrDefault();

            if (cheapestStore == null) continue;

            if (!storeBaskets.ContainsKey(cheapestStore.Id))
                storeBaskets[cheapestStore.Id] = new();

            var key = $"{item.ProductId}_{cheapestStore.Id}";
            effectivePrices.TryGetValue(key, out var ep);
            productMap.TryGetValue(item.ProductId, out var product);

            storeBaskets[cheapestStore.Id].Add(new BasketItemDto(
                item.ProductId, item.ProductName.Length > 0 ? item.ProductName : product?.Name ?? item.ProductId,
                product?.Brand ?? "", item.Quantity, ep.price, ep.price * item.Quantity,
                ep.hasDeal, ep.hasDeal ? ep.originalPrice : null));
        }

        if (storeBaskets.Count <= 1) return null; // Not a multi-store option

        var storeBasketDtos = storeBaskets.Select(kvp =>
        {
            storeObjects.TryGetValue(kvp.Key, out var store);
            retailerMap.TryGetValue(store?.RetailerId ?? "", out var retailer);
            var subTotal = kvp.Value.Sum(i => i.LineTotal);
            var travelCost = prefs.TransportMode == "driving" && store != null
                ? _travelCost.CalculateFuelCost(store.DistanceKm, prefs.VehicleConsumptionLPer100km, prefs.FuelPricePerLitre)
                : 0m;
            return new StoreBasketDto(kvp.Key, store?.Name ?? kvp.Key, retailer?.Name ?? kvp.Key,
                retailer?.Color ?? "#333", kvp.Value, subTotal, store?.DistanceKm ?? 0, travelCost);
        }).ToList();

        var productTotal = storeBasketDtos.Sum(s => s.SubTotal);
        var totalTravel = storeBasketDtos.Sum(s => s.TravelCost);
        var totalDistance = storeBasketDtos.Sum(s => s.DistanceKm);
        var totalMinutes = storeBasketDtos.Sum(s =>
        {
            storeObjects.TryGetValue(s.StoreId, out var st);
            return _travelCost.EstimateTravelMinutes(s.DistanceKm, prefs.TransportMode) + (st?.EstimatedShoppingMinutes ?? 20);
        });

        return new ShoppingOptionDto(
            "multi_cheapest", "Cheapest Products (Multi-Store)", "💰",
            storeBasketDtos, productTotal, totalTravel, 0m, productTotal + totalTravel,
            storeBaskets.Count, 0m, totalMinutes, totalDistance,
            $"Split across {storeBaskets.Count} stores for lowest product prices", false);
    }

    private List<BasketItemDto> BuildBasketItems(string storeId, List<ShoppingListItemDto> items,
        Dictionary<string, Product> productMap,
        Dictionary<string, (decimal price, bool hasDeal, decimal originalPrice)> effectivePrices)
    {
        return items.Select(item =>
        {
            var key = $"{item.ProductId}_{storeId}";
            if (!effectivePrices.TryGetValue(key, out var ep)) return null;
            productMap.TryGetValue(item.ProductId, out var product);
            return (BasketItemDto?)new BasketItemDto(
                item.ProductId, item.ProductName.Length > 0 ? item.ProductName : product?.Name ?? item.ProductId,
                product?.Brand ?? "", item.Quantity, ep.price, ep.price * item.Quantity,
                ep.hasDeal, ep.hasDeal ? ep.originalPrice : null);
        }).Where(i => i != null).Select(i => i!.Value).ToList();
    }

    private ShoppingOptionDto SelectRecommendation(List<ShoppingOptionDto> options, string priority,
        decimal? budget, UserPreferencesDto prefs)
    {
        var withinBudget = budget.HasValue
            ? options.Where(o => o.TotalEstimatedCost <= budget.Value).ToList()
            : options;

        var pool = withinBudget.Any() ? withinBudget : options;

        return priority switch
        {
            "saveMoney" => pool.OrderBy(o => o.TotalEstimatedCost).First(),
            "saveTravel" => pool.OrderBy(o => o.TravelCost).ThenBy(o => o.TotalDistanceKm).First(),
            "saveTime" => pool.OrderBy(o => o.EstimatedTotalMinutes).First(),
            "supportLocal" => pool.OrderByDescending(o => o.StoreBaskets.Any(s => s.RetailerName == "Fresh Corner")).ThenBy(o => o.TotalEstimatedCost).First(),
            _ => pool.OrderBy(o => o.TotalEstimatedCost + o.TravelCost * 1.5m).First()
        };
    }

    private string BuildRecommendationReason(ShoppingOptionDto recommended, List<ShoppingOptionDto> all,
        string priority, decimal? budget)
    {
        var cheapestTotal = all.Min(o => o.TotalEstimatedCost);
        var lowestTravel = all.Min(o => o.TravelCost);
        var fastest = all.Min(o => o.EstimatedTotalMinutes);

        var parts = new List<string>();

        if (budget.HasValue && recommended.TotalEstimatedCost <= budget.Value)
            parts.Add($"keeps you within your R{budget.Value:F0} budget");

        if (recommended.StoreCount == 1)
            parts.Add("requires only one stop");

        if (recommended.TravelCost == lowestTravel)
            parts.Add($"has the lowest estimated travel cost (R{recommended.TravelCost:F2})");

        if (recommended.TotalEstimatedCost == cheapestTotal)
            parts.Add($"is the cheapest overall at R{recommended.TotalEstimatedCost:F2}");

        if (recommended.EstimatedTotalMinutes == fastest)
            parts.Add($"is the fastest option at ~{recommended.EstimatedTotalMinutes} minutes");

        var multiStore = all.FirstOrDefault(o => o.Key == "multi_cheapest");
        if (multiStore != null && recommended.Key != "multi_cheapest")
        {
            var productSaving = multiStore.ProductTotal - recommended.ProductTotal;
            var travelExtra = multiStore.TravelCost - recommended.TravelCost;
            if (productSaving < 0 && travelExtra > 0)
                parts.Add($"splitting across {multiStore.StoreCount} stores would save R{Math.Abs(productSaving):F2} on products but cost R{travelExtra:F2} more in travel");
        }

        var reason = parts.Any()
            ? $"We recommend {recommended.Label} because it {string.Join(" and ", parts)}."
            : $"We recommend {recommended.Label} as the best match for your {priority} priority.";

        return reason;
    }

    private static string GetBudgetStatus(decimal total, decimal budget)
    {
        var pct = total / budget * 100;
        return pct <= 90 ? "under" : pct <= 100 ? "close" : "over";
    }
}
