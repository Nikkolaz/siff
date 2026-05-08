namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    // Interface for a generic repository that handles objects of type T
    public interface IGenericRepository<T> where T : class
    {
        // Method to retrieve an object by ID
        Task<T> Get(int id);
        // Method to retrieve all objects of type T
        Task<IReadOnlyList<T>> GetAll();
        // Method to add an object of type T to the repository
        Task<T> Add(T entity);
        // Method to check if an object with a given ID exists in the repository
        Task<bool> Exists(int id);
        // Method to update an object of type T in the repository
        Task Update(T entity);
        // Method to delete an object of type T from the repository
        Task Delete(T entity);
    }
}
