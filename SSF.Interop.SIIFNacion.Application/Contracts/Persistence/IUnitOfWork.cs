using Microsoft.EntityFrameworkCore.Storage;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    /// <summary>
    /// Interface for the Unit of Work pattern, which provides a way to interact with multiple repositories in a transactional manner.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Saves all changes made in the current unit of work to the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Save();

        /// <summary>
        /// Begins a new transaction asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation that returns the transaction.</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Adds a range of entities to the current unit of work asynchronously.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to add.</typeparam>
        /// <param name="entities">The entities to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        /// <summary>
        /// Updates a range of entities in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to update.</typeparam>
        /// <param name="entities">The entities to update.</param>
        void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        /// <summary>
        /// Removes a range of entities from the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to remove.</typeparam>
        /// <param name="entities">The entities to remove.</param>
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}
