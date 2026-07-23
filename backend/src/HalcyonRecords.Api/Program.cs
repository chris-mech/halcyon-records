using System.Diagnostics;
using HalcyonRecords.Api.Common;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
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

builder.Services.AddApiRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.RunAsync();
