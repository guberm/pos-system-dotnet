using System.ComponentModel.DataAnnotations;

namespace POSSystem.Models;

/// <summary>
/// Represents a product category in the POS system
/// </summary>
public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}