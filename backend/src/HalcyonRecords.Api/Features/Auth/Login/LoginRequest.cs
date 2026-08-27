using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed record LoginRequest(
    [property: MaxLength(256)] [property: Description("A valid email address.")] string Email,
    string Password
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "email": "john.doe@example.com",
                "password": "P@ssw0rd123!"
            }
            """
        )!;
}
