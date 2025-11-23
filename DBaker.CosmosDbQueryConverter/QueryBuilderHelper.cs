using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace DBaker.CosmosDbQueryConverter;

/// <summary>
/// Internal helper class that provides core functionality for building and parameterizing Cosmos DB SQL queries.
/// </summary>
/// <remarks>
/// This class handles the transformation of templated query strings and expression-based queries into
/// properly parameterized <see cref="QueryDefinition"/> objects with safe parameter binding.
/// </remarks>
internal static class QueryBuilderHelper
{
    /// <summary>
    /// Builds a parameterized query by replacing numbered placeholders with parameter names and handling collection expansion.
    /// </summary>
    /// <param name="query">
    /// The SQL query string containing numbered placeholders in the form {0}, {1}, etc.
    /// </param>
    /// <param name="values">
    /// The values to substitute into the query. If a value is an <see cref="IEnumerable"/> (excluding strings and byte arrays),
    /// it will be expanded into multiple parameters for use in IN clauses.
    /// </param>
    /// <returns>
    /// A tuple containing the updated query string with parameter names and a dictionary of parameters to be bound.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when a placeholder in the query references a value index that is out of range.
    /// </exception>
    public static (string Query, Dictionary<string, object?> Parameters) BuildQuery(string query, params object?[] values)
    {
        var paramIndex = 0;
        var usedParamIndexes = new Dictionary<int, string>();
        Dictionary<string, object?> parameters = [];
        var regex = new Regex(@"\{(\d+)\}");

        var updatedQuery = regex.Replace(query, match =>
        {
            int valueIndex = int.Parse(match.Groups[1].Value);

            if (valueIndex >= values.Length)
                throw new ArgumentException($"No parameter provided for index {{{valueIndex}}}.");

            var value = values[valueIndex];

            if (usedParamIndexes.TryGetValue(valueIndex, out var existingParamName))
                return existingParamName;

            if (value is IEnumerable enumerable and not string and not byte[])
            {
                List<string> paramNames = [];

                foreach (var item in enumerable)
                {
                    var name = $"@p{paramIndex++}";
                    paramNames.Add(name);
                    parameters.Add(name, item);
                }

                var joinedNames = $"({string.Join(", ", paramNames)})";
                usedParamIndexes[valueIndex] = joinedNames;
                return joinedNames;
            }

            var paramName = $"@p{paramIndex++}";
            usedParamIndexes[valueIndex] = paramName;
            parameters.Add(paramName, value);
            return paramName;
        });

        return (updatedQuery, parameters);
    }

    /// <summary>
    /// Extracts values from an expression and converts it to a parameterized query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method processes a lambda expression that returns a <see cref="FormattableString"/>.
    /// Interpolated expressions are analyzed to determine if they represent field references (which are resolved to JSON paths)
    /// or parameter values (which are extracted and bound separately).
    /// </para>
    /// <para>
    /// Field references are identified by the <see cref="IFieldNameResolver"/> and replaced directly in the query without parameterization.
    /// Other expressions are evaluated and their values are passed as parameters.
    /// </para>
    /// </remarks>
    /// <param name="lambda">
    /// A lambda expression that returns a <see cref="FormattableString"/>. Typically compiled from an interpolated string expression.
    /// </param>
    /// <param name="fieldNameResolver">
    /// The resolver to use for identifying and converting field references to JSON property paths.
    /// </param>
    /// <returns>
    /// A tuple containing the final parameterized query and a dictionary of extracted parameter values.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the expression body is not a valid <see cref="FormattableString"/> or does not have the expected structure.
    /// </exception>
    public static (string Query, Dictionary<string, object?> Parameters) ParameterizeExpression(
        LambdaExpression lambda, 
        IFieldNameResolver fieldNameResolver)
    {
        //Strings in the form $"string content {variable} {var2}" are translated into this inside an expression:
        //FormattableStringFactory.Create($"string content {0} {1}", variable, var2)
        if (lambda.Body is not MethodCallExpression methodCall ||
            methodCall.Method.Name != "Create")
            throw new InvalidOperationException("Expected interpolated string to become FormattableStringFactory.Create(...)");

        if (methodCall.Arguments[0] is not ConstantExpression formatArg)
            throw new InvalidOperationException("Format string is not a constant");

        var formatString = (string)formatArg.Value!;
        var argExprs = ((NewArrayExpression)methodCall.Arguments[1]).Expressions;

        var paramValues = new List<object?>();
        var rewrittenArgs = argExprs.Select(expr =>
        {
            if (fieldNameResolver.TryResolve(expr, out var path))
                return path;

            paramValues.Add(Expression.Lambda(expr).Compile().DynamicInvoke());
            return $"{{{paramValues.Count - 1}}}";
        }).ToArray();

        var rewrittenQuery = string.Format(CultureInfo.InvariantCulture, formatString, rewrittenArgs);
        return BuildQuery(rewrittenQuery, [.. paramValues]);
    }
}