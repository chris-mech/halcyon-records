using HalcyonRecords.Api.Common.Logging;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Api.Infrastructure.Search;
using HalcyonRecords.Api.Infrastructure.Seed;
using HalcyonRecords.Api.Infrastructure.Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public static class JobRunner
{
    private const string JobArgument = "--job";

    public static bool TryGetRequestedJob(string[] args, out string jobName)
    {
        var index = Array.IndexOf(args, JobArgument);

        if (index >= 0 && index + 1 < args.Length)
        {
            jobName = args[index + 1];
            return true;
        }

        jobName = string.Empty;
        return false;
    }

    public static async Task<int> RunAsync(
        IServiceProvider services,
        string jobName,
        CancellationToken cancellationToken = default
    )
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(JobRunner));

        logger.JobStarting(jobName);

        try
        {
            switch (jobName)
            {
                case "migrate":
                    await provider
                        .GetRequiredService<ApplicationDbContext>()
                        .Database.MigrateAsync(cancellationToken);
                    break;

                case "seed":
                    await DbSeeder.SeedAsync(
                        provider.GetRequiredService<ApplicationDbContext>(),
                        provider.GetRequiredService<UserManager<User>>(),
                        provider.GetRequiredService<IOptions<ShopOptions>>(),
                        provider.GetRequiredService<TimeProvider>(),
                        cancellationToken
                    );
                    break;

                case "reindex":
                    await provider
                        .GetRequiredService<MeilisearchIndexer>()
                        .RebuildAsync(
                            provider.GetRequiredService<ApplicationDbContext>(),
                            cancellationToken
                        );
                    break;

                case "account-maintenance":
                    await provider
                        .GetRequiredService<DemoAccountCleaner>()
                        .RemoveStaleAccountsAsync(cancellationToken);
                    await provider
                        .GetRequiredService<ShowcaseAccountResetter>()
                        .ResetShowcaseAccountAsync(cancellationToken);
                    break;

                case "restock":
                    await provider
                        .GetRequiredService<AlbumRestocker>()
                        .RestockAsync(cancellationToken);
                    break;

                default:
                    logger.JobUnknown(jobName);
                    return 1;
            }
        }
        catch (Exception ex)
        {
            logger.JobFailed(ex, jobName);
            return 1;
        }

        logger.JobCompleted(jobName);
        return 0;
    }
}
