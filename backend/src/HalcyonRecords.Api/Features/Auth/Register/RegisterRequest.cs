using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed record RegisterRequest(
    [property: MaxLength(100)] string FirstName,
    [property: MaxLength(100)] string LastName,
    [property: MaxLength(256)] [property: Description("A valid email address.")] string Email,
    string Password
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "firstName": "John",
                "lastName": "Doe",
                "email": "john.doe@example.com",
                "password": "P@ssw0rd123!"
            }
            """
        )!;
}
