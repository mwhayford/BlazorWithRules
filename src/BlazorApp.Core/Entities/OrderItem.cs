using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Core.Entities;

/// <summary>
/// Order item entity representing individual items within an order
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// Foreign key to Order
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to Order
    /// </summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string ProductName { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    [StringLength(100)]
    public string? ProductSku { get; set; }

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price at time of order
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total price for this line item (Quantity * UnitPrice)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Product description at time of order
    /// </summary>
    [StringLength(1000)]
    public string? ProductDescription { get; set; }

    /// <summary>
    /// Product category
    /// </summary>
    [StringLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Discount amount applied to this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// Tax amount for this item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; } = 0;
}
