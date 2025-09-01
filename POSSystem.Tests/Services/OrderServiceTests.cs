namespace POSSystem.Tests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly POSContext _context;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<POSContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new POSContext(options);
        _orderService = new OrderService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var category = new Category { Id = 1, Name = "Test Category", Description = "Test Description" };
        var product = new Product 
        { 
            Id = 1, 
            Name = "Test Product", 
            Price = 10.00m, 
            StockQuantity = 100, 
            CategoryId = 1,
            Category = category
        };
        var customer = new Customer 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john.doe@test.com" 
        };

        _context.Categories.Add(category);
        _context.Products.Add(product);
        _context.Customers.Add(customer);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ReturnsOrderWithItems()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = await _orderService.CreateOrderAsync(order);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.CustomerId);
        Assert.Single(result.Items);
        Assert.Equal(20.00m, result.TotalAmount); // 2 * 10.00
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateOrderAsync_InsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 200 } // More than available stock
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orderService.CreateOrderAsync(order));
    }

    [Fact]
    public async Task GetOrderAsync_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1 }
            }
        };
        var createdOrder = await _orderService.CreateOrderAsync(order);

        // Act
        var result = await _orderService.GetOrderAsync(createdOrder.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdOrder.Id, result.Id);
    }

    [Fact]
    public async Task GetOrderAsync_NonExistentOrder_ReturnsNull()
    {
        // Act
        var result = await _orderService.GetOrderAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CompleteOrderAsync_PendingOrder_UpdatesStatusToCompleted()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1 }
            }
        };
        var createdOrder = await _orderService.CreateOrderAsync(order);

        // Act
        await _orderService.CompleteOrderAsync(createdOrder.Id);

        // Assert
        var updatedOrder = await _orderService.GetOrderAsync(createdOrder.Id);
        Assert.Equal(OrderStatus.Completed, updatedOrder?.Status);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidOrder_UpdatesPaymentInfo()
    {
        // Arrange
        var order = new Order
        {
            CustomerId = 1,
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1 }
            }
        };
        var createdOrder = await _orderService.CreateOrderAsync(order);

        // Act
        await _orderService.ProcessPaymentAsync(createdOrder.Id, "CASH");

        // Assert
        var updatedOrder = await _orderService.GetOrderAsync(createdOrder.Id);
        Assert.Equal("CASH", updatedOrder?.PaymentMethod);
        Assert.True(updatedOrder?.PaymentProcessed);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
