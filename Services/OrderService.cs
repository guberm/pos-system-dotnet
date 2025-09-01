using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using POSSystem.Models;

namespace POSSystem.Services;

/// <summary>
/// Service implementation for Order operations
/// Demonstrates business logic, data validation, and transaction management
/// </summary>
public class OrderService : IOrderService
{
    private readonly POSContext _context;
    private readonly ILogger<OrderService> _logger;
    private const decimal TAX_RATE = 0.08m; // 8% tax rate

    public OrderService(POSContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Creating new order with {ItemCount} items", request.Items.Count);

            // Validate products and check stock
            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id);

            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Keys);
                throw new InvalidOperationException($"Products not found: {string.Join(", ", missingIds)}");
            }

            // Check stock availability
            foreach (var item in request.Items)
            {
                var product = products[item.ProductId];
                if (product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");
                }
            }

            // Create order
            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(),
                CustomerId = request.CustomerId,
                PaymentMethod = request.PaymentMethod,
                DiscountAmount = request.DiscountAmount,
                Notes = request.Notes,
                Status = OrderStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get Order ID

            // Create order items and calculate totals
            decimal subTotal = 0;
            foreach (var itemRequest in request.Items)
            {
                var product = products[itemRequest.ProductId];
                var lineTotal = product.Price * itemRequest.Quantity - (itemRequest.DiscountAmount ?? 0);

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = lineTotal,
                    DiscountAmount = itemRequest.DiscountAmount ?? 0,
                    Notes = itemRequest.Notes
                };

                _context.OrderItems.Add(orderItem);
                subTotal += lineTotal;

                // Update product stock
                product.StockQuantity -= itemRequest.Quantity;
                product.ModifiedDate = DateTime.UtcNow;
            }

            // Calculate totals
            order.SubTotal = subTotal;
            order.TaxAmount = (subTotal - request.DiscountAmount) * TAX_RATE;
            order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Order {OrderNumber} created successfully with total amount {TotalAmount:C}",
                order.OrderNumber, order.TotalAmount);

            return await GetOrderByIdAsync(order.Id) ?? order;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order");
            throw;
        }
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        var oldStatus = order.Status;
        order.Status = status;

        if (status == OrderStatus.Completed && order.CompletedDate == null)
        {
            order.CompletedDate = DateTime.UtcNow;
        }

        // Handle stock restoration for cancelled orders
        if (status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
        {
            await RestoreStockForCancelledOrderAsync(orderId);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus}",
            orderId, oldStatus, status);

        return order;
    }

    public async Task<decimal> CalculateOrderTotalAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        var subTotal = order.OrderItems.Sum(oi => oi.LineTotal);
        var taxAmount = (subTotal - order.DiscountAmount) * TAX_RATE;

        return subTotal + taxAmount - order.DiscountAmount;
    }

    public async Task<bool> ProcessPaymentAsync(int orderId, PaymentMethod paymentMethod, string? paymentReference)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot process payment for order in {order.Status} status");

        // Simulate payment processing logic
        var paymentSuccessful = await SimulatePaymentProcessingAsync(order.TotalAmount, paymentMethod);

        if (paymentSuccessful)
        {
            order.PaymentMethod = paymentMethod;
            order.PaymentReference = paymentReference;
            order.Status = OrderStatus.Processing;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment processed successfully for order {OrderId} using {PaymentMethod}",
                orderId, paymentMethod);
        }
        else
        {
            _logger.LogWarning("Payment failed for order {OrderId}", orderId);
        }

        return paymentSuccessful;
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var lastOrder = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith($"ORD-{today}"))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastOrder != null)
        {
            var lastSequence = lastOrder.OrderNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastSequence, out int parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"ORD-{today}-{sequence:D4}";
    }

    private async Task RestoreStockForCancelledOrderAsync(int orderId)
    {
        var orderItems = await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        foreach (var item in orderItems)
        {
            item.Product.StockQuantity += item.Quantity;
            item.Product.ModifiedDate = DateTime.UtcNow;
        }

        _logger.LogInformation("Stock restored for cancelled order {OrderId}", orderId);
    }

    private static async Task<bool> SimulatePaymentProcessingAsync(decimal amount, PaymentMethod paymentMethod)
    {
        // Simulate async payment processing
        await Task.Delay(100);

        // Simulate occasional payment failures (10% failure rate)
        var random = new Random();
        return random.NextDouble() > 0.1;
    }
}