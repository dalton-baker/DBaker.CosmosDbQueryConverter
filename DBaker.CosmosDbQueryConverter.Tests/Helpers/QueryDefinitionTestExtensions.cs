using Microsoft.Azure.Cosmos;

namespace DBaker.CosmosQueryDefinitionBuilder.Tests.Helpers;

public static class QueryDefinitionTestExtensions
{
    public static Dictionary<string, object> GetDictionaryQueryParameters(this QueryDefinition query)
       => query.GetQueryParameters().ToDictionary(k => k.Name, v => v.Value);
}