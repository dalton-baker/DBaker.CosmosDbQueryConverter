using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;

namespace DBaker.CosmosQueryDefinitionBuilder;

public static partial class CosmosDbQuery
{
    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T>(Expression<Func<T, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2>(Expression<Func<T1, T2, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3>(Expression<Func<T1, T2, T3, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <inheritdoc cref="ConvertLambdaExpression" />
    public static QueryDefinition Convert<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, FormattableString>> query, IFieldNameResolver? fieldNameResolver = null)
        => ConvertLambdaExpression(query, fieldNameResolver);

    /// <summary>
    /// Converts a strongly-typed expression returning a <see cref="FormattableString"/> into a parameterized <see cref="QueryDefinition"/>.
    /// Enables compile-time safety for field names, automatic resolution of JSON property names,
    /// and proper handling of embedded parameters.
    /// </summary>
    /// <param name="query">
    /// An expression that returns an interpolated string, typically written as:
    /// <c>(x) =&gt; $"SELECT * FROM c WHERE c.name = {x.Name}"</c>.
    /// Fields accessed within the expression body are resolved to their Cosmos DB JSON field paths,
    /// using <see cref="JsonPropertyAttribute"/>, <see cref="JsonPropertyNameAttribute"/>, or the property name.
    /// Embedded values are treated as parameters and bound safely.
    /// </param>
    /// <param name="fieldNameResolver">
    /// Optional custom resolver for converting property expressions to JSON field paths.
    /// If null, a <see cref="DefaultFieldNameResolver"/> with camel-casing enabled is used.
    /// </param>
    /// <returns>
    /// A fully parameterized <see cref="QueryDefinition"/> with all expression-based fields and values substituted appropriately.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the expression body is not a valid interpolated string.
    /// </exception>
    internal static QueryDefinition ConvertLambdaExpression(LambdaExpression query, IFieldNameResolver? fieldNameResolver = null)
    {
        var (builtQuery, parameters) = QueryBuilderHelper.ParameterizeExpression(
            query,
            fieldNameResolver ?? new DefaultFieldNameResolver());

        var queryDef = new QueryDefinition(builtQuery);
        foreach (var param in parameters)
            queryDef.WithParameter(param.Key, param.Value);

        return queryDef;
    }
} 