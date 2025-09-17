using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Core.Entities;

/// <summary>
/// Order entity representing customer orders
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// Order number (unique identifier for business use)
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string OrderNumber { get; set; }

    /// <summary>
    /// Foreign key to User
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Order date
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total amount of the order
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Shipping address
    /// </summary>
    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Billing address
    /// </summary>
    [StringLength(500)]
    public string? BillingAddress { get; set; }

    /// <summary>
    /// Order notes
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Date when order was shipped
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Date when order was delivered
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Navigation property for order items
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
