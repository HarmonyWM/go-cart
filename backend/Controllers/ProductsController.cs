using MaliMove.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MaliMove.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "")
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Query parameter 'q' is required");
        return Ok(await _productService.SearchProductsAsync(q));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpGet("{id}/alternatives")]
    public async Task<IActionResult> GetAlternatives(string id) =>
        Ok(await _productService.GetBrandAlternativesAsync(id));
}
