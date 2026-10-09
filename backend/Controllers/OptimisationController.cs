using MaliMove.Api.DTOs;
using MaliMove.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MaliMove.Api.Controllers;

[ApiController]
[Route("api")]
public class OptimisationController : ControllerBase
{
    private readonly IShoppingOptimisationService _optimisationService;
    public OptimisationController(IShoppingOptimisationService optimisationService) =>
        _optimisationService = optimisationService;

    [HttpPost("optimisation/calculate")]
    public async Task<IActionResult> Calculate([FromBody] OptimiseRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest("Shopping list cannot be empty");
        return Ok(await _optimisationService.OptimiseAsync(request));
    }

    [HttpPost("shopping-lists/optimise")]
    public async Task<IActionResult> OptimiseList([FromBody] OptimiseRequest request) =>
        await Calculate(request);
}
