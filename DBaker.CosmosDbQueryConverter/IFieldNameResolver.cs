using System.Linq.Expressions;

namespace DBaker.CosmosDbQueryConverter;

/// <summary>
/// Defines a contract for resolving expression field names to their corresponding JSON property paths in Cosmos DB queries.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for translating property access expressions
/// into their JSON field representation, accounting for custom property name attributes and naming conventions.
/// </remarks>
public interface IFieldNameResolver
{
    /// <summary>
    /// Attempts to resolve an expression to its corresponding JSON property path.
    /// </summary>
    /// <param name="expression">
    /// The expression to resolve. Typically a property access expression from a lambda expression.
    /// Can be null or a parameter expression.
    /// </param>
    /// <param name="path">
    /// When this method returns <c>true</c>, contains the resolved JSON property path (e.g., "c.firstName").
    /// When this method returns <c>false</c>, this will be <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the expression was successfully resolved to a path; otherwise, <c>false</c>.
    /// </returns>
    bool TryResolve(Expression? expression, out string? path);
}
