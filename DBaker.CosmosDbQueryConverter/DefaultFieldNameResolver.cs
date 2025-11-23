using System.Linq.Expressions;
using System.Reflection;

namespace DBaker.CosmosQueryDefinitionBuilder;

/// <summary>
/// Default implementation of <see cref="IFieldNameResolver"/> that resolves property expressions to JSON property paths.
/// </summary>
/// <remarks>
/// <para>
/// This resolver supports multiple naming conventions:
/// <list type="bullet">
/// <item><description>Custom JSON property names via <c>JsonPropertyAttribute</c> (Newtonsoft.Json)</description></item>
/// <item><description>Custom JSON property names via <c>JsonPropertyNameAttribute</c> (System.Text.Json)</description></item>
/// <item><description>Defaults to the property name (camel case by default, but overridable)</description></item>
/// </list>
/// </para>
/// <para>
/// It handles member expressions (property access), unary expressions (nullable conversions), and parameter expressions.
/// </para>
/// </remarks>
/// <param name="useCamelCase">
/// If <c>true</c>, property names without custom JSON attributes are converted to camel case. 
/// If <c>false</c>, property names are used as-is. Default is <c>true</c>.
/// </param>
public class DefaultFieldNameResolver(bool useCamelCase = true) : IFieldNameResolver
{
    /// <inheritdoc />
    public bool TryResolve(Expression? expression, out string? path)
    {
        switch (expression)
        {
            case ParameterExpression p when !string.IsNullOrEmpty(p.Name):
                path = p.Name;
                return true;
            case MemberExpression m when TryResolve(m.Expression, out path):
                path += $".{GetJsonPropertyName(m.Member)}";
                return true;
            case UnaryExpression u when u.Operand is MemberExpression m && TryResolve(m.Expression, out path):
                path += $".{GetJsonPropertyName(m.Member)}";
                return true;
            case UnaryExpression u when u.NodeType == ExpressionType.Convert:
                return TryResolve(u.Operand, out path);
            default:
                path = null;
                return false;
        }
    }

    /// <summary>
    /// Gets the JSON property name for a given member, respecting custom JSON property attributes.
    /// </summary>
    /// <param name="member">The member (property or field) to get the JSON name for.</param>
    /// <returns>
    /// The custom JSON property name if present, otherwise the member name converted to camel case
    /// (if the resolver is configured for camel case) or the member name as-is.
    /// </returns>
    protected virtual string GetJsonPropertyName(MemberInfo member)
    {
        foreach (var attr in member.GetCustomAttributes(false))
        {
            var typeName = attr.GetType().Name;
            if (typeName == "JsonPropertyAttribute") //Newtonsoft
            {
                var prop = attr.GetType().GetProperty("PropertyName");
                var value = prop?.GetValue(attr) as string;
                if (!string.IsNullOrEmpty(value))
                    return value!;
            }
            else if (typeName == "JsonPropertyNameAttribute") //System.Text.Json
            {
                var prop = attr.GetType().GetProperty("Name");
                var value = prop?.GetValue(attr) as string;
                if (!string.IsNullOrEmpty(value))
                    return value!;
            }
        }

        if (useCamelCase)
            return ToCamelCase(member.Name);
        else
            return member.Name;
    }

    /// <summary>
    /// Converts a string to camel case by lowercasing the first character.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>The input string with the first character lowercased, or the original string if null or empty.</returns>
    protected virtual string ToCamelCase(string s)
        => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
}