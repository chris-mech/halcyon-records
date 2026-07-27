using HalcyonRecords.Api.Common;
using HalcyonRecords.Api.Common.Endpoints;
using HalcyonRecords.Api.Common.RateLimiting;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Api.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Sqids;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddSqlServerDbContext<ApplicationDbContext>("halcyonrecords");

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddSingleton(
    new SqidsEncoder<int>(
        new SqidsOptions { Alphabet = "gvCi8aFhyjVq1ELk5tSwWURGOMp42ubnosl3z9IHZe6TcABQ7XdrDPNxfKYJ0m", MinLength = 6 }
    )
);

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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
}

app.UseExceptionHandler();
app.UseRateLimiter();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.RunAsync();
