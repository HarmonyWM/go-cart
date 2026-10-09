namespace MaliMove.Api.Models;

// --- Order Models ---

public enum OrderStatus
{
    Placed, ShopperAssigned, ShoppingInProgress, OutForDelivery, Delivered, Cancelled
}

public enum ShoppingMethod { ShopMyself, PersonalShopper }

public class OrderItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public bool HasDeal { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsSubstituted { get; set; }
    public string? SubstitutionNote { get; set; }
}

public class OrderStoreBasket
{
    public string StoreId { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string RetailerName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public double DistanceKm { get; set; }
    public decimal TravelCost { get; set; }
}

public class DeliveryDetails
{
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string DeliveryWindow { get; set; } = string.Empty;
    public string? DeliveryNotes { get; set; }
}

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public ShoppingMethod Method { get; set; }
    public List<OrderStoreBasket> Baskets { get; set; } = new();
    public decimal ProductTotal { get; set; }
    public decimal TravelCost { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal GrandTotal { get; set; }
    public DeliveryDetails? Delivery { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public string? ShopperId { get; set; }
    public string? PaymentReference { get; set; }
    public string PaymentStatus { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderStatusEvent> StatusHistory { get; set; } = new();
}

public class OrderStatusEvent
{
    public OrderStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// --- Existing Models ---

public class Retailer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
}

public class Store
{
    public string Id { get; set; } = string.Empty;
    public string RetailerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double DistanceKm { get; set; }
    public int EstimatedShoppingMinutes { get; set; }
}

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string> Alternatives { get; set; } = new();
}

public class ProductPrice
{
    public string ProductId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Deal
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string RetailerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DealPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DiscountPercent { get; set; }
    public decimal SavingsAmount { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ShoppingListItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool IsPurchased { get; set; }
}

public class ShoppingList
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "My Shopping List";
    public List<ShoppingListItem> Items { get; set; } = new();
    public decimal? Budget { get; set; }
    public string Priority { get; set; } = "bestOverall";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class UserPreferences
{
    public string UserId { get; set; } = "default";
    public List<string> PreferredStoreIds { get; set; } = new();
    public double MaxTravelDistanceKm { get; set; } = 15;
    public string TransportMode { get; set; } = "driving";
    public string FuelType { get; set; } = "petrol95";
    public double VehicleConsumptionLPer100km { get; set; } = 7.2;
    public decimal FuelPricePerLitre { get; set; } = 24.50m;
    public bool WillingToSwitchBrands { get; set; } = true;
    public string DefaultPriority { get; set; } = "bestOverall";
}
