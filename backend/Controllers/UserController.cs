using MaliMove.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MaliMove.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private static UserPreferencesDto _preferences = new(
        PreferredStoreIds: new List<string>(),
        MaxTravelDistanceKm: 15,
        TransportMode: "driving",
        FuelType: "petrol95",
        VehicleConsumptionLPer100km: 7.2,
        FuelPricePerLitre: 24.50m,
        WillingToSwitchBrands: true,
        DefaultPriority: "bestOverall");

    [HttpGet("preferences")]
    public IActionResult GetPreferences() => Ok(_preferences);

    [HttpPut("preferences")]
    public IActionResult UpdatePreferences([FromBody] UserPreferencesDto preferences)
    {
        _preferences = preferences;
        return Ok(_preferences);
    }
}
