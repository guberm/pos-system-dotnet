using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystem.Models;

/// <summary>
/// Represents a customer in the POS system
/// </summary>
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(150)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    // Computed property
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}