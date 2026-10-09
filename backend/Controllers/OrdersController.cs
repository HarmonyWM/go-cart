using MaliMove.Api.DTOs;
using MaliMove.Api.Interfaces;
using MaliMove.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MaliMove.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        if (request.Baskets == null || !request.Baskets.Any())
            return BadRequest("Order must contain at least one basket.");
        if (request.Method != "ShopMyself" && request.Method != "PersonalShopper")
            return BadRequest("Method must be ShopMyself or PersonalShopper.");
        if (request.Method == "PersonalShopper" && request.Delivery == null)
            return BadRequest("Delivery details are required for personal shopper orders.");

        try
        {
            var order = await _orderService.PlaceOrderAsync(request);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber)
    {
        var order = await _orderService.GetOrderAsync(orderNumber);
        return order == null ? NotFound() : Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _orderService.GetAllOrdersAsync());

    [HttpPost("{orderId}/status")]
    public async Task<IActionResult> UpdateStatus(string orderId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            if (!Enum.TryParse<OrderStatus>(request.Status, out var status))
                return BadRequest($"Invalid status: {request.Status}");
            var order = await _orderService.UpdateStatusAsync(orderId, status, request.Message);
            return Ok(order);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("payment/demo")]
    public async Task<IActionResult> DemoPayment([FromBody] DemoPaymentRequest request) =>
        Ok(await _orderService.ProcessDemoPaymentAsync(request));

    [HttpPost("substitution/approve")]
    public async Task<IActionResult> ApproveSubstitution([FromBody] SubstitutionRequest request)
    {
        try { return Ok(await _orderService.ApproveSubstitutionAsync(request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}

public record UpdateStatusRequest(string Status, string Message);
