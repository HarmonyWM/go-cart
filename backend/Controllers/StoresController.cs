using GoCart.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GoCart.Api.Controllers;

[ApiController]
[Route("api/stores")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    public StoresController(IStoreService storeService) => _storeService = storeService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _storeService.GetAllStoresAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        return store == null ? NotFound() : Ok(store);
    }
}
