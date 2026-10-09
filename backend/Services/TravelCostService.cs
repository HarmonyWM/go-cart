using MaliMove.Api.Interfaces;

namespace MaliMove.Api.Services;

public class TravelCostService : ITravelCostService
{
    public decimal CalculateFuelCost(double distanceKm, double consumptionLPer100km, decimal fuelPricePerLitre)
    {
        var litresUsed = (distanceKm / 100.0) * consumptionLPer100km;
        // Round trip
        return Math.Round((decimal)(litresUsed * 2) * fuelPricePerLitre, 2);
    }

    public int EstimateTravelMinutes(double distanceKm, string transportMode) => transportMode switch
    {
        "walking" => (int)Math.Ceiling(distanceKm / 5.0 * 60),
        "publicTransport" => (int)Math.Ceiling(distanceKm / 25.0 * 60) + 10,
        _ => (int)Math.Ceiling(distanceKm / 40.0 * 60)
    };
}
