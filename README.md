# DBaker.CosmosDbQueryConverter

A type-safe, parameterized query builder for Azure Cosmos DB with lambda expression support and automatic JSON property mapping.

## Features

- 🔒 **Type-safe queries** with compile-time validation using lambda expressions
- 🎯 **Automatic property mapping** from C# property names to JSON field names (supports `JsonProperty` and `JsonPropertyName` attributes)
- 🛡️ **SQL injection protection** through automatic parameterization
- 📦 **Array expansion** for `IN` clauses
- 🔗 **Nested property support** for complex document structures

## Installation

```bash
dotnet add package DBaker.CosmosDbQueryConverter
```

## Usage

### Lambda Expressions (Recommended)

The most powerful approach - get IntelliSense, refactoring support, and automatic JSON property name resolution:

```csharp
using DBaker.CosmosDbQueryConverter;

// Simple query with type safety
var query = CosmosDbQuery.Convert((MyDocument c) =>
    $"SELECT * FROM {c} WHERE {c.Status} = {myStatus}");

// Nested properties
var query = CosmosDbQuery.Convert((MyDocument c) =>
    $"SELECT * FROM {c} WHERE {c.Address.City} = {myCity}");

// Array expansion for IN clauses
var query = CosmosDbQuery.Convert((MyDocument c) =>
    $"SELECT * FROM {c} WHERE {c.Category} IN {myCategories}");

// Multi-line queries with JOINs
var query = CosmosDbQuery.Convert((MyDocument doc, SubDocument sub) =>
    $"""
    SELECT *
    FROM {doc}
    JOIN sub IN {sub}
    WHERE {doc.Id} = {myId}
      AND {sub.Status} = {myStatus}
    """)
```

Properties are automatically mapped using `JsonProperty` (Newtonsoft.Json) or `JsonPropertyName` (System.Text.Json) attributes, with camelCase as the default.

### Interpolated Strings

Quick and convenient for simple queries:

```csharp
var query = CosmosDbQuery.Convert(
    $"SELECT * FROM c WHERE c.id = {myId} AND c.status = {myStatus}");

// Arrays work here too
var query = CosmosDbQuery.Convert(
    $"SELECT * FROM c WHERE c.tag IN {myTags}")
```

### Placeholder-based Queries

Useful when working with dynamic queries or parameter arrays:

```csharp
var query = CosmosDbQuery.Convert(
    "SELECT * FROM c WHERE c.category IN {0} AND c.price >= {1}",
    myCategories,
    minPrice)
```

## How It Works

All values are automatically converted to named parameters (`@p0`, `@p1`, etc.) to prevent SQL injection. Arrays and other `IEnumerable` types are expanded into multiple parameters for `IN` clauses.

**Before:**
```csharp
$"SELECT * FROM c WHERE c.status IN {myStatuses}"
```

**After:**
```sql
SELECT * FROM c WHERE c.status IN (@p0, @p1, @p2)
```

## Custom Field Name Resolution

Customize how property names are resolved:

```csharp
public class MyFieldNameResolver : IFieldNameResolver
{
    public string ResolveFieldName(MemberExpression memberExpression)
    {
        // Your custom logic here
        return memberExpression.Member.Name.ToLowerInvariant();
    }
}

var query = CosmosDbQuery.Convert(
    (MyDocument c) => $"SELECT * FROM {c} WHERE {c.Status} = {myStatus}",
    new MyFieldNameResolver());
```