using System.Linq.Expressions;

namespace SSF.Interop.SIIFNacion.Application.Contracts.Persistence
{
    // Interface for a specification pattern for objects of type T
    public interface ISpecification<T>
    {
        // Property to store the criteria for filtering objects of type T
        Expression<Func<T, bool>> Criteria { get; }
        // Property to store the expressions for eager-loading related data for objects of type T
        List<Expression<Func<T, object>>> Includes { get; }
        // Property to store the strings for eager-loading related data for objects of type T
        List<string> IncludeStrings { get; }
        // Property to store the expression for ordering objects of type T
        Expression<Func<T, object>> OrderBy { get; }
        // Property to store the expression for ordering objects of type T in descending order
        Expression<Func<T, object>> OrderByDescending { get; }
        // Property to store the expression for grouping objects of type T
        Expression<Func<T, object>> GroupBy { get; }
        // Property to store the number of objects to take
        int Take { get; }
        // Property to store the number of objects to skip
        int Skip { get; }
        // Property to indicate whether paging is enabled
        bool IsPagingEnabled { get; }
    }
}
