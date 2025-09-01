using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystem.Models;

/// <summary>
/// Represents an individual item within an order
/// </summary>
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    [Required]
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [StringLength(200)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}