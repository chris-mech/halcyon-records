using System.Reflection;
using System.Text;
using HalcyonRecords.Api.Features.Auth.Register;
using HalcyonRecords.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class PasswordPolicySchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            context.JsonPropertyInfo?.AttributeProvider is not PropertyInfo { Name: "Password" }
            || context.JsonPropertyInfo.DeclaringType != typeof(RegisterRequest)
        )
        {
            return Task.CompletedTask;
        }

        var policy = context
            .ApplicationServices.GetRequiredService<IOptions<PasswordPolicyOptions>>()
            .Value;

        schema.MinLength = policy.RequiredLength;
        schema.Pattern = BuildPattern(policy);
        schema.Description = BuildDescription(policy);

        return Task.CompletedTask;
    }

    private static string BuildPattern(PasswordPolicyOptions policy)
    {
        var lookaheads = new StringBuilder();

        if (policy.RequireLowercase)
        {
            lookaheads.Append("(?=.*[a-z])");
        }

        if (policy.RequireUppercase)
        {
            lookaheads.Append("(?=.*[A-Z])");
        }

        if (policy.RequireDigit)
        {
            lookaheads.Append("(?=.*\\d)");
        }

        if (policy.RequireNonAlphanumeric)
        {
            lookaheads.Append("(?=.*[^a-zA-Z0-9])");
        }

        return $"^{lookaheads}.+$";
    }

    private static string BuildDescription(PasswordPolicyOptions policy)
    {
        var requirements = new List<string> { $"at least {policy.RequiredLength} characters" };

        if (policy.RequireLowercase)
        {
            requirements.Add("a lowercase letter");
        }

        if (policy.RequireUppercase)
        {
            requirements.Add("an uppercase letter");
        }

        if (policy.RequireDigit)
        {
            requirements.Add("a digit");
        }

        if (policy.RequireNonAlphanumeric)
        {
            requirements.Add("a non-alphanumeric character");
        }

        if (policy.RequiredUniqueChars > 1)
        {
            requirements.Add($"at least {policy.RequiredUniqueChars} unique characters");
        }

        return $"Must contain {string.Join(", ", requirements)}.";
    }
}
