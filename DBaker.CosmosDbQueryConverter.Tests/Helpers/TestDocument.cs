using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DBaker.CosmosQueryDefinitionBuilder.Tests.Helpers;

public class TestDocument
{
    public string Prop { get; set; } = string.Empty;
    public List<string> PropList { get; set; } = [];

    [JsonProperty("newtonsoft_prop")]
    public string NewtonsoftProp { get; set; } = string.Empty;

    [JsonPropertyName("system_text")]
    public string SystemTextProp { get; set; } = string.Empty;
    public TestSubDocument SubDoc { get; set; } = new();
}
