using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSystem.Data;
using POSSystem.Models;

namespace POSSystem.Controllers;

/// <summary>
/// API Controller for Product management operations
/// Demonstrates REST API design, data validation, and error handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly POSContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(POSContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all active products with optional filtering and pagination
    /// </summary>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="searchTerm">Search in product name or description</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>List of products</returns>
    [HttpGet]
    public async Task<ActionResult<ProductListResponse>> GetProducts(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (pageSize > 100) pageSize = 100;
            if (page < 1) page = 1;

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // Apply filters
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    p.SKU.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate,
                    ModifiedDate = p.ModifiedDate
                })
                .ToListAsync();

            var response = new ProductListResponse
            {
                Products = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate,
                    ModifiedDate = p.ModifiedDate
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product</returns>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if SKU already exists
            if (await _context.Products.AnyAsync(p => p.SKU == request.SKU))
            {
                return BadRequest($"Product with SKU '{request.SKU}' already exists");
            }

            // Verify category exists
            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            {
                return BadRequest($"Category with ID {request.CategoryId} not found or inactive");
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                SKU = request.SKU,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId})", product.Name, product.Id);

            // Return the created product with category information
            var createdProduct = await GetProduct(product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, createdProduct.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update request</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
            {
                return NotFound($"Product with ID {id} not found");
            }

            // Check if SKU change conflicts with existing product
            if (request.SKU != product.SKU && await _context.Products.AnyAsync(p => p.SKU == request.SKU && p.Id != id))
            {
                return BadRequest($"Product with SKU '{request.SKU}' already exists");
            }

            // Verify category exists if changed
            if (request.CategoryId != product.CategoryId && !await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            {
                return BadRequest($"Category with ID {request.CategoryId} not found or inactive");
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.SKU = request.SKU;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            product.CategoryId = request.CategoryId;
            product.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId})", product.Name, product.Id);

            var updatedProduct = await GetProduct(product.Id);
            return Ok(updatedProduct.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    /// <summary>
    /// Soft delete a product (mark as inactive)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            product.IsActive = false;
            product.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted (soft): {ProductName} (ID: {ProductId})", product.Name, product.Id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    /// <param name="threshold">Stock threshold (default: 10)</param>
    /// <returns>List of low stock products</returns>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts([FromQuery] int threshold = 10)
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    IsActive = p.IsActive,
                    CreatedDate = p.CreatedDate,
                    ModifiedDate = p.ModifiedDate
                })
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock products");
            return StatusCode(500, "An error occurred while retrieving low stock products");
        }
    }
}

// DTOs for API responses
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class ProductListResponse
{
    public List<ProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
}
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
}
