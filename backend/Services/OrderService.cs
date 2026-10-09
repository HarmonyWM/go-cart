using MaliMove.Api.DTOs;
using MaliMove.Api.Interfaces;
using MaliMove.Api.Models;

namespace MaliMove.Api.Services;

public class OrderService : IOrderService
{
    // In-memory store for MVP — replace with EF Core persistence in production
    private static readonly List<Order> _orders = new();
    private static readonly object _lock = new();

    public Task<OrderDto> PlaceOrderAsync(PlaceOrderRequest request)
    {
        // Validate total on backend
        var calculatedTotal = request.ProductTotal + request.TravelCost + request.DeliveryFee + request.ServiceFee;
        if (Math.Abs(calculatedTotal - request.GrandTotal) > 0.10m)
            throw new InvalidOperationException($"Total mismatch: expected R{calculatedTotal:F2}, received R{request.GrandTotal:F2}");

        var method = request.Method == "PersonalShopper" ? ShoppingMethod.PersonalShopper : ShoppingMethod.ShopMyself;

        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            OrderNumber = GenerateOrderNumber(),
            Method = method,
            Baskets = request.Baskets.Select(b => new OrderStoreBasket
            {
                StoreId = b.StoreId,
                StoreName = b.StoreName,
                RetailerName = b.RetailerName,
                Color = b.Color,
                SubTotal = b.SubTotal,
                DistanceKm = b.DistanceKm,
                TravelCost = b.TravelCost,
                Items = b.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Brand = i.Brand,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal,
                    HasDeal = i.HasDeal,
                    OriginalPrice = i.OriginalPrice
                }).ToList()
            }).ToList(),
            ProductTotal = request.ProductTotal,
            TravelCost = request.TravelCost,
            DeliveryFee = request.DeliveryFee,
            ServiceFee = request.ServiceFee,
            GrandTotal = request.GrandTotal,
            Delivery = request.Delivery == null ? null : new DeliveryDetails
            {
                Address = request.Delivery.Address,
                ContactNumber = request.Delivery.ContactNumber,
                DeliveryWindow = request.Delivery.DeliveryWindow,
                DeliveryNotes = request.Delivery.DeliveryNotes
            },
            Status = OrderStatus.Placed,
            StatusHistory = new List<OrderStatusEvent>
            {
                new() { Status = OrderStatus.Placed, Message = "Order placed successfully.", Timestamp = DateTime.UtcNow }
            }
        };

        // Auto-assign mock shopper for personal shopper orders
        if (method == ShoppingMethod.PersonalShopper)
        {
            order.ShopperId = "shopper-mock-001";
            order.Status = OrderStatus.ShopperAssigned;
            order.StatusHistory.Add(new OrderStatusEvent
            {
                Status = OrderStatus.ShopperAssigned,
                Message = "A shopper has been assigned to your order. [DEMO]",
                Timestamp = DateTime.UtcNow.AddSeconds(2)
            });
        }

        lock (_lock) { _orders.Add(order); }

        return Task.FromResult(MapToDto(order));
    }

    public Task<OrderDto?> GetOrderAsync(string orderNumber)
    {
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(o =>
                o.OrderNumber == orderNumber || o.Id == orderNumber);
            return Task.FromResult(order == null ? null : MapToDto(order));
        }
    }

    public Task<List<OrderDto>> GetAllOrdersAsync()
    {
        lock (_lock) { return Task.FromResult(_orders.Select(MapToDto).ToList()); }
    }

    public Task<OrderDto> UpdateStatusAsync(string orderId, OrderStatus status, string message)
    {
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId || o.OrderNumber == orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found");
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            order.StatusHistory.Add(new OrderStatusEvent { Status = status, Message = message });
            return Task.FromResult(MapToDto(order));
        }
    }

    public Task<DemoPaymentResult> ProcessDemoPaymentAsync(DemoPaymentRequest request)
    {
        // DEMO ONLY — no real payment processing
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(o => o.Id == request.OrderId || o.OrderNumber == request.OrderId);
            if (order == null)
                return Task.FromResult(new DemoPaymentResult(false, "", "Order not found"));

            var reference = $"DEMO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
            order.PaymentReference = reference;
            order.PaymentStatus = "paid_demo";
            order.UpdatedAt = DateTime.UtcNow;
            order.StatusHistory.Add(new OrderStatusEvent
            {
                Status = order.Status,
                Message = $"Demo payment processed. Reference: {reference} [DEMO — not a real transaction]",
                Timestamp = DateTime.UtcNow
            });

            return Task.FromResult(new DemoPaymentResult(true, reference,
                "⚠️ DEMO payment processed. No real money was charged."));
        }
    }

    public Task<OrderDto> ApproveSubstitutionAsync(SubstitutionRequest request)
    {
        lock (_lock)
        {
            var order = _orders.FirstOrDefault(o => o.Id == request.OrderId || o.OrderNumber == request.OrderId)
                ?? throw new KeyNotFoundException($"Order {request.OrderId} not found");

            foreach (var basket in order.Baskets)
            {
                var item = basket.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
                if (item != null)
                {
                    item.IsSubstituted = true;
                    item.SubstitutionNote = request.SubstitutionNote;
                    item.OriginalPrice = item.UnitPrice;
                    item.UnitPrice = request.NewUnitPrice;
                    item.LineTotal = request.NewUnitPrice * item.Quantity;
                    basket.SubTotal = basket.Items.Sum(i => i.LineTotal);
                }
            }

            order.ProductTotal = order.Baskets.Sum(b => b.SubTotal);
            order.GrandTotal = order.ProductTotal + order.TravelCost + order.DeliveryFee + order.ServiceFee;
            order.UpdatedAt = DateTime.UtcNow;
            order.StatusHistory.Add(new OrderStatusEvent
            {
                Status = order.Status,
                Message = $"Substitution approved for product {request.ProductId}: {request.SubstitutionNote}"
            });

            return Task.FromResult(MapToDto(order));
        }
    }

    private static string GenerateOrderNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var seq = (_orders.Count + 1).ToString("D4");
        return $"MM-{date}-{seq}";
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id, o.OrderNumber, o.Method.ToString(),
        o.Baskets.Select(b => new OrderStoreBasketDto(
            b.StoreId, b.StoreName, b.RetailerName, b.Color,
            b.Items.Select(i => new OrderItemDto(
                i.ProductId, i.ProductName, i.Brand, i.Quantity,
                i.UnitPrice, i.LineTotal, i.HasDeal, i.OriginalPrice,
                i.IsSubstituted, i.SubstitutionNote)).ToList(),
            b.SubTotal, b.DistanceKm, b.TravelCost)).ToList(),
        o.ProductTotal, o.TravelCost, o.DeliveryFee, o.ServiceFee, o.GrandTotal,
        o.Delivery == null ? null : new DeliveryDetailsDto(
            o.Delivery.Address, o.Delivery.ContactNumber,
            o.Delivery.DeliveryWindow, o.Delivery.DeliveryNotes),
        o.Status.ToString(), o.PaymentStatus,
        o.CreatedAt,
        o.StatusHistory.Select(e => new OrderStatusEventDto(
            e.Status.ToString(), e.Message, e.Timestamp)).ToList());
}
