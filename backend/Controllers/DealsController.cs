using GoCart.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoCart.Api.Controllers;

[ApiController]
[Route("api/deals")]
public class DealsController : ControllerBase
{
    private readonly IDealService _dealService;
    public DealsController(IDealService dealService) => _dealService = dealService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? storeId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null) =>
        Ok(await _dealService.GetAllDealsAsync(storeId, category, search));

    [HttpPost("matching-list")]
    public async Task<IActionResult> GetMatchingDeals([FromBody] List<string> productIds) =>
        Ok(await _dealService.GetDealsMatchingListAsync(productIds));
}
