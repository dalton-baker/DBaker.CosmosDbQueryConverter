using Microsoft.Azure.Cosmos;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace DBaker.CosmosDbQueryConverter;

public static partial class CosmosDbQuery
{
    /// <summary>
    /// Builds a parameterized <see cref="QueryDefinition"/> from a SQL query template containing
    /// numbered placeholder tokens like <c>{0}</c>, <c>{1}</c>, etc., along with a corresponding set of values.
    /// </summary>
    /// <param name="query">
    /// The SQL query string containing numbered placeholders in the form <c>{0}</c>, <c>{1}</c>, etc.
    /// These placeholders will be replaced with auto-generated parameter names (e.g., <c>@p0</c>, <c>@p1</c>).
    /// </param>
    /// <param name="values">
    /// The values to substitute into the query. Each index in the array corresponds to a placeholder in the query.
    /// If a value is an <see cref="IEnumerable"/>, it will be expanded into multiple parameters for use in <c>IN</c> clauses.
    /// </param>
    /// <returns>
    /// A fully constructed <see cref="QueryDefinition"/> with all placeholders replaced and parameters bound.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the query references an index that is not present in the <paramref name="values"/> array.
    /// </exception>
    public static QueryDefinition Convert(string query, params object?[] values)
    {
        var (updatedQuery, parameters) = QueryBuilderHelper.BuildQuery(query, values);

        var queryDef = new QueryDefinition(updatedQuery);
        foreach (var param in parameters)
            queryDef.WithParameter(param.Key, param.Value);

        return queryDef;
    }

    /// <summary>
    /// Converts an interpolated SQL query into a parameterized <see cref="QueryDefinition"/>.
    /// Each interpolation expression is extracted as a parameter, and the resulting query string is safely formatted
    /// with numbered placeholders (<c>{0}</c>, <c>{1}</c>, etc.) for substitution.
    /// </summary>
    /// <param name="formattable">
    /// An interpolated SQL-like string (e.g., <c>$"SELECT * FROM c WHERE c.id = {id}"</c>).
    /// Embedded expressions will be automatically extracted and passed as query parameters.
    /// If any expression is an <see cref="IEnumerable"/> (excluding <see cref="string"/> and <see cref="byte[]"/>),
    /// it will be expanded for use in <c>IN</c> clauses.
    /// </param>
    /// <returns>
    /// A fully constructed <see cref="QueryDefinition"/> with parameters bound to their corresponding values.
    /// </returns>
    public static QueryDefinition Convert(FormattableString formattable)
    {
        var args = formattable.GetArguments();
        string rewrittenQuery = string.Format(
            CultureInfo.InvariantCulture,
            formattable.Format,
            [.. args.Select((_, i) => $"{{{i}}}")]);
        return Convert(rewrittenQuery, args);
    }
}
