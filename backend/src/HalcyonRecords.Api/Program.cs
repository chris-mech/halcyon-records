using System.Diagnostics;
using FluentValidation;
using HalcyonRecords.Api.Common;
using HalcyonRecords.Api.Common.Behaviours;
using HalcyonRecords.Api.Common.Caching;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Common.RateLimiting;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;
using HalcyonRecords.Api.Features.Search;
using HalcyonRecords.Api.Features.Search.Search;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Api.Infrastructure.Search;
using HalcyonRecords.Api.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<IntegerSchemaTransformer>();
});

builder.Services.Configure<SearchOptions>(
    builder.Configuration.GetSection(SearchOptions.SectionName)
);
builder.Services.Configure<RelatedAlbumsOptions>(
    builder.Configuration.GetSection(RelatedAlbumsOptions.SectionName)
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    await DbSeeder.SeedAsync(dbContext);

    var indexer = scope.ServiceProvider.GetRequiredService<MeilisearchIndexer>();
    await indexer.RebuildAsync(dbContext);

    app.MapOpenApi();

    app.MapPost(
        "/api/dev/search/reindex",
        async (ApplicationDbContext db, MeilisearchIndexer indexer, CancellationToken ct) =>
        {
            await indexer.RebuildAsync(db, ct);
            return Results.Ok();
        }
    );
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.RunAsync();

public partial class Program;
