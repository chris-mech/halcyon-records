using System.Text.Json.Nodes;

namespace HalcyonRecords.Api.Common.OpenApi;

public interface IOpenApiExampleProvider
{
    static abstract JsonNode Example { get; }
}
