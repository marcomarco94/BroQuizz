using System.Linq.Expressions;

namespace DataAccess.Repository;

/// <summary>
///     Defines the contract for a base repository, providing methods for interacting with data.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IBaseRepository<T>
{
    /// <summary>
    ///     Retrieves all entities asynchronously.
    /// </summary>
    /// <returns>A collection of entities.</returns>
    Task<IEnumerable<T?>> GetAllAsync();

    /// <summary>
    ///     Retrieves an entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the entity.</param>
    /// <returns>The entity, if found; otherwise, null.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    ///     Retrieves entities based on a condition asynchronously.
    /// </summary>
    /// <param name="expression">The condition to apply.</param>
    /// <returns>A collection of entities that satisfy the condition.</returns>
    Task<IEnumerable<T?>> GetByConditionAsync(Expression<Func<T?, bool>> expression);

    /// <summary>
    ///     Adds a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    Task AddAsync(T? entity);

    /// <summary>
    ///     Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    Task UpdateAsync(T? entity);

    /// <summary>
    ///     Deletes an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    Task DeleteAsync(T? entity);
}