using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlazorApp.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the Repository
    /// </summary>
    /// <param name="context">Database context</param>
    public Repository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Get entity by ID asynchronously
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity or null if not found</returns>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Get all entities asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find entities by expression asynchronously
    /// </summary>
    /// <param name="expression">Filter expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities matching the criteria</returns>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(expression).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Add new entity asynchronously
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added entity</returns>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Update existing entity asynchronously
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entity</returns>
    public virtual Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    /// <summary>
    /// Delete entity by ID asynchronously (soft delete)
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return false;

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        
        await UpdateAsync(entity, cancellationToken);
        return true;
    }

    /// <summary>
    /// Save changes to the database asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get queryable for advanced queries
    /// </summary>
    /// <returns>IQueryable for the entity</returns>
    public virtual IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    /// <summary>
    /// Get entities with paging
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged collection of entities</returns>
    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get total count of entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count</returns>
    public virtual async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Check if entity exists by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists</returns>
    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
