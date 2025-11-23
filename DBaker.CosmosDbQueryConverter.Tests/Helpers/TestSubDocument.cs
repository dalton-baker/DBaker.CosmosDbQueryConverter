using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DBaker.CosmosDbQueryConverter.Tests.Helpers;

public class TestSubDocument
{
    public string Prop { get; set; } = string.Empty;

    [JsonProperty("newtonsoft_prop")]
    public string NewtonsoftProp { get; set; } = string.Empty;

    [JsonPropertyName("system_text")]
    public string SystemTextProp { get; set; } = string.Empty;
}