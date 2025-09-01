namespace POSSystem.Tests.Controllers;

public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<POSContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<POSContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });

        _client = _factory.CreateClient();
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<POSContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var category = new Category { Id = 1, Name = "Electronics", Description = "Electronic devices" };
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "Gaming laptop",
            Price = 999.99m,
            StockQuantity = 10,
            CategoryId = 1,
            Category = category
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", 
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products");
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.Contains(products, p => p.Name == "Laptop");
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(product);
        Assert.Equal("Laptop", product.Name);
    }

    [Fact]
    public async Task GetProduct_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/999");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ValidProduct_ReturnsCreated()
    {
        // Arrange
        var newProduct = new
        {
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 29.99m,
            StockQuantity = 50,
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<Product>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(createdProduct);
        Assert.Equal("Mouse", createdProduct.Name);
    }
}
