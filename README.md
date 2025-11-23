# DBaker.CosmosDbQueryConverter

Write native Cosmos DB SQL queries with automatic parameterization. No query builder abstractions - just Cosmos SQL with safe parameter handling.

## Features

- **Write real SQL** - Use the full power of Cosmos DB SQL syntax, not a limited query builder API
- **Automatic parameterization** - Parameter injection protection without manual parameter management
- **IntelliSense support** - Lambda expressions give you autocomplete and prevent property name typos
- **Smart array expansion** - Arrays automatically expand into `IN` clause parameters
- **JSON property mapping** - Automatically resolves `JsonProperty` and `JsonPropertyName` attributes

## Installation

```bash
dotnet add package DBaker.CosmosDbQueryConverter
```

## Usage

### Lambda Expressions (Recommended)

Write real Cosmos DB SQL with IntelliSense and automatic property mapping:

```csharp
using DBaker.CosmosDbQueryConverter;

// Simple query with parameters and property names
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
    JOIN {sub} IN {doc.SubDoc}
    WHERE {doc.Id} = {myId}
      AND {sub.Status} = {myStatus}
    """)
```

Properties are automatically mapped using `JsonProperty` (Newtonsoft.Json) or `JsonPropertyName` (System.Text.Json) attributes. If neither exist we fall back to the camel cased property name.

### Interpolated Strings

Write native Cosmos SQL with interpolated values - all parameters are automatically handled:

```csharp
var query = CosmosDbQuery.Convert(
    $"SELECT * FROM c WHERE c.id = {myId} AND c.status = {myStatus}");

// Arrays work here too
var query = CosmosDbQuery.Convert(
    $"SELECT * FROM c WHERE c.tag IN {myTags}")
```

### Placeholder-based Queries

For dynamic query building with parameter arrays:

```csharp
var query = CosmosDbQuery.Convert(
    "SELECT * FROM c WHERE c.category IN {0} AND c.price >= {1}",
    myCategories,
    minPrice)
```

## How It Works

You write native Cosmos DB SQL queries. The library handles all the parameterization work automatically - converting values to named parameters (`@p0`, `@p1`, etc.) and expanding arrays into multiple parameters for `IN` clauses.

**Before:**
```csharp
$"SELECT * FROM {c} WHERE {c.Status} IN {myStatuses}"
```

**After:**
```sql
SELECT * FROM c WHERE c.status IN (@p0, @p1, @p2)
```

### Executing Queries

The `Convert` methods return a standard `QueryDefinition` that you pass directly to the Cosmos DB SDK:

```csharp
var query = CosmosDbQuery.Convert((MyDocument c) =>
    $"SELECT * FROM {c} WHERE {c.Status} = {myStatus}");

var queryIterator = container.GetItemQueryIterator<MyDocument>(query);
```

## Custom Field Name Resolution

### Disable Camel Casing

By default, property names are converted to camel case. To use property names as-is:

```csharp
var query = CosmosDbQuery.Convert(
    (MyDocument c) => $"SELECT * FROM {c} WHERE {c.Status} = {myStatus}",
    new DefaultFieldNameResolver(useCamelCase: false));
```

### Custom Resolver

Create your own resolver by implementing `IFieldNameResolver` or inheriting from `DefaultFieldNameResolver`:

```csharp
public class MyFieldNameResolver : DefaultFieldNameResolver
{
    protected override string GetJsonPropertyName(MemberInfo member)
    {
        // Your custom logic here
        return member.Name.ToLowerInvariant();
    }
}

var query = CosmosDbQuery.Convert(
    (MyDocument c) => $"SELECT * FROM {c} WHERE {c.Status} = {myStatus}",
    new MyFieldNameResolver());
```