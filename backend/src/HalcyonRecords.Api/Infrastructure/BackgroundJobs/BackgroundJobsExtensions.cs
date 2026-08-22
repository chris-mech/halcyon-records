using Coravel;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.BackgroundJobs;

public static class BackgroundJobsExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiBackgroundJobs(IConfiguration configuration)
        {
            services.Configure<AccountMaintenanceOptions>(
                configuration.GetSection(AccountMaintenanceOptions.SectionName)
            );
            services.AddScoped<DemoAccountCleaner>();
            services.AddScoped<ShowcaseAccountResetter>();
            services.AddScoped<AccountMaintenanceJob>();

            services.Configure<AlbumRestockOptions>(
                configuration.GetSection(AlbumRestockOptions.SectionName)
            );
            services.AddScoped<AlbumRestocker>();
            services.AddScoped<AlbumRestockJob>();

            services.AddScheduler();

            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseApiBackgroundJobs()
        {
            app.Services.UseScheduler(scheduler =>
            {
                var accountOptions = app
                    .Services.GetRequiredService<IOptions<AccountMaintenanceOptions>>()
                    .Value;
                scheduler.Schedule<AccountMaintenanceJob>().Cron(accountOptions.CronSchedule);

                var restockOptions = app
                    .Services.GetRequiredService<IOptions<AlbumRestockOptions>>()
                    .Value;
                scheduler.Schedule<AlbumRestockJob>().Cron(restockOptions.CronSchedule);
            });

            return app;
        }

        public WebApplication MapBackgroundJobsDevEndpoints()
        {
            app.MapPost(
                    "/api/dev/demo-accounts/run-maintenance",
                    async (
                        DemoAccountCleaner cleaner,
                        ShowcaseAccountResetter resetter,
                        CancellationToken ct
                    ) =>
                    {
                        var removed = await cleaner.RemoveStaleAccountsAsync(ct);
                        await resetter.ResetShowcaseAccountAsync(ct);
                        return Results.Ok(new { accountsRemoved = removed });
                    }
                )
                .ExcludeFromDescription();

            app.MapPost(
                    "/api/dev/albums/restock",
                    async (AlbumRestocker restocker, CancellationToken ct) =>
                    {
                        var restocked = await restocker.RestockAsync(ct);
                        return Results.Ok(new { albumsRestocked = restocked });
                    }
                )
                .ExcludeFromDescription();

            return app;
        }
    }
}
