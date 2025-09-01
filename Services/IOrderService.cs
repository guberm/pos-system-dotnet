using POSSystem.Models;

namespace POSSystem.Services;

/// <summary>
/// Interface for Order Service operations
/// </summary>
public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<decimal> CalculateOrderTotalAsync(int orderId);
    Task<bool> ProcessPaymentAsync(int orderId, PaymentMethod paymentMethod, string?paymentReference);
}
/// <summary>
/// Request model for creating new orders
/// </summary>
public class CreateOrderRequest
{
    public int? CustomerId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
}
/// <summary>
/// Request model for order items
/// </summary>
public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? Notes { get; set; }
}