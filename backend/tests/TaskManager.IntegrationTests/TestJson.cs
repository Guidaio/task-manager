using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskManager.IntegrationTests;

internal static class TestJson
{
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
