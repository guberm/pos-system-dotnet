using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystem.Models;

/// <summary>
/// Represents a customer order in the POS system
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    [StringLength(100)]
    public string? PaymentReference { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    // Navigation properties
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5
}
public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    DigitalWallet = 4,
    Check = 5
}