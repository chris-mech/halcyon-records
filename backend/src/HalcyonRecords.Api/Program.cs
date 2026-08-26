using System.Diagnostics;
using System.Text.Json.Serialization;
using FluentValidation;
using HalcyonRecords.Api.Common;
using HalcyonRecords.Api.Common.Behaviours;
using HalcyonRecords.Api.Common.Caching;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.RateLimiting;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;
using HalcyonRecords.Api.Features.Search;
using HalcyonRecords.Api.Features.Search.Search;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Api.Infrastructure.BackgroundJobs;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Api.Infrastructure.Search;
using HalcyonRecords.Api.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddSqlServerDbContext<ApplicationDbContext>("halcyonrecords");

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<AlbumSqidEncoder>();
builder.Services.AddSingleton<ArtistSqidEncoder>();
builder.Services.AddSingleton<SuggestedTermsProvider>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddApiHybridCache(builder.Configuration);

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviour<,>));
    config.AddOpenBehavior(typeof(CachingBehaviour<,>));
    config.LicenseKey = builder.Configuration["MediatR:LicenseKey"];
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApiMeilisearch(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddApiAuth(builder.Configuration);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddSchemaTransformer<IntegerSchemaTransformer>();
    options.AddSchemaTransformer<ExampleSchemaTransformer>();
    options.AddOperationTransformer<SortEnumParameterTransformer>();
    options.AddOperationTransformer<RequireBearerSecurityOperationTransformer>();
    options.AddDocumentTransformer<GlobalErrorResponseDocumentTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title = "Halcyon Records API";
            document.Info.Version = "v1";
            document.Info.Description = "REST API for the Halcyon Records online record shop.";
            return Task.CompletedTask;
        }
    );
});

builder.Services.Configure<SearchOptions>(
    builder.Configuration.GetSection(SearchOptions.SectionName)
);
builder.Services.Configure<RelatedAlbumsOptions>(
    builder.Configuration.GetSection(RelatedAlbumsOptions.SectionName)
);
builder.Services.Configure<ShopOptions>(builder.Configuration.GetSection(ShopOptions.SectionName));

builder.Services.AddApiBackgroundJobs(builder.Configuration);

var app = builder.Build();

app.UseApiBackgroundJobs();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var shopOptions = scope.ServiceProvider.GetRequiredService<IOptions<ShopOptions>>();
    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    await DbSeeder.SeedAsync(dbContext, userManager, shopOptions, timeProvider);

    var indexer = scope.ServiceProvider.GetRequiredService<MeilisearchIndexer>();
    await indexer.RebuildAsync(dbContext);

    app.MapPost(
            "/api/dev/search/reindex",
            async (ApplicationDbContext db, MeilisearchIndexer indexer, CancellationToken ct) =>
            {
                await indexer.RebuildAsync(db, ct);
                return Results.Ok();
            }
        )
        .ExcludeFromDescription();

    app.MapBackgroundJobsDevEndpoints();
}

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Halcyon Records API");
    options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch);
});

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.RunAsync();

public partial class Program;
