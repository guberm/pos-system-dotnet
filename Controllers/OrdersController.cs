using Microsoft.AspNetCore.Mvc;
using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem.Controllers;

/// <summary>
/// API Controller for Order management operations
/// Demonstrates complex business logic, transaction handling, and service integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }
    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order</returns>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest("Order must contain at least one item");
            }
            var order = await _orderService.CreateOrderAsync(request);
            var orderDto = MapToOrderDto(order);
            _logger.LogInformation("Order created successfully: {OrderNumber}", order.OrderNumber);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating order");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while creating the order");
        }
    }
    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }
            var orderDto = MapToOrderDto(order);
            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }
    /// <summary>
    /// Get orders by date range
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <returns>List of orders</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByDateRange(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.Date;
            var end = endDate ?? DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
            if (start > end)
            {
                return BadRequest("Start date cannot be greater than end date");
            }
            var orders = await _orderService.GetOrdersByDateRangeAsync(start, end);
            var orderSummaries = orders.Select(MapToOrderSummaryDto).ToList();
            return Ok(orderSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by date range");
            return StatusCode(500, "An error occurred while retrieving orders");
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated order</returns>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _orderService.UpdateOrderStatusAsync(id, request.Status);
            var orderDto = MapToOrderDto(order);

            _logger.LogInformation("Order {OrderId} status updated to {Status}", id, request.Status);

            return Ok(orderDto);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status {OrderId}", id);
            return StatusCode(500, "An error occurred while updating the order status");
        }
    }

    /// <summary>
    /// Process payment for an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Payment processing request</param>
    /// <returns>Payment result</returns>
    [HttpPost("{id}/payment")]
    public async Task<ActionResult<PaymentResultDto>> ProcessPayment(int id, [FromBody] ProcessPaymentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _orderService.ProcessPaymentAsync(id, request.PaymentMethod, request.PaymentReference);

            var result = new PaymentResultDto
            {
                OrderId = id,
                Success = success,
                PaymentMethod = request.PaymentMethod,
                PaymentReference = request.PaymentReference,
                ProcessedAt = DateTime.UtcNow,
                Message = success ? "Payment processed successfully" : "Payment processing failed"
            };
            if (success)
            {
                _logger.LogInformation("Payment processed successfully for order {OrderId}", id);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Payment failed for order {OrderId}", id);
                return BadRequest(result);
            }
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", id);
            return StatusCode(500, "An error occurred while processing the payment");
        }
    }
    
    /// <summary>
    /// Calculate order total
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order total calculation</returns>
    [HttpGet("{id}/total")]
    public async Task<ActionResult<OrderTotalDto>> CalculateOrderTotal(int id)
    {
        try
        {
            var total = await _orderService.CalculateOrderTotalAsync(id);

            var result = new OrderTotalDto
            {
                OrderId = id,
                TotalAmount = total,
                CalculatedAt = DateTime.UtcNow
            };
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating order total {OrderId}", id);
            return StatusCode(500, "An error occurred while calculating the order total");
        }
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.FullName,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            PaymentReference = order.PaymentReference,
            OrderDate = order.OrderDate,
            CompletedDate = order.CompletedDate,
            Notes = order.Notes,
            Items = order.OrderItems?.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "",
                ProductSKU = oi.Product?.SKU ?? "",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                LineTotal = oi.LineTotal,
                DiscountAmount = oi.DiscountAmount,
                Notes = oi.Notes
            }).ToList() ?? new List<OrderItemDto>()
        };
    }

    private static OrderSummaryDto MapToOrderSummaryDto(Order order)
    {
        return new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.Customer?.FullName,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            OrderDate = order.OrderDate,
            ItemCount = order.OrderItems?.Count ?? 0
        };
    }
}

// DTOs for API requests and responses
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class ProcessPaymentRequest
{
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
}

public class PaymentResultDto
{
    public int OrderId { get; set; }
    public bool Success { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class OrderTotalDto
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CalculatedAt { get; set; }
}