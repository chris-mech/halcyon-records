using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class TagDescriptionDocumentTransformer : IOpenApiDocumentTransformer
{
    private static readonly Dictionary<string, string> s_descriptions = new()
    {
        ["Albums"] =
            "Browse album listings and detail pages, including related albums and the weekly staff-pick cover story.",
        ["Artists"] =
            "Browse artist listings and detail pages, including each artist's discography.",
        ["Auth"] =
            "Register, log in, and manage the current session, including token refresh and logout.",
        ["Carts"] = "Read and synchronize the current shopping cart.",
        ["Decades"] = "Browse albums grouped by release decade.",
        ["Genres"] = "Browse albums grouped by genre.",
        ["Orders"] = "Create and retrieve orders.",
        ["Search"] = "Full-text search across the catalogue, including autocomplete suggestions.",
    };

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (document.Tags is null)
        {
            return Task.CompletedTask;
        }

        foreach (var tag in document.Tags)
        {
            if (s_descriptions.TryGetValue(tag.Name!, out var description))
            {
                tag.Description = description;
            }
        }

        return Task.CompletedTask;
    }
}
