using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Core.Entities;

/// <summary>
/// Base entity class that provides common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the entity was deleted (soft delete)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User who deleted the entity
    /// </summary>
    public string? DeletedBy { get; set; }
}
