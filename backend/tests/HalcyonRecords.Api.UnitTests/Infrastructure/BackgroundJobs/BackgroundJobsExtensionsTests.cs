using Coravel.Scheduling.Schedule.Interfaces;
using HalcyonRecords.Api.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HalcyonRecords.Api.UnitTests.Infrastructure.BackgroundJobs;

public class BackgroundJobsExtensionsTests
{
    [Fact]
    public void UseApiBackgroundJobs_MaintenanceJobsExcluded_StillSchedulesMeilisearchWarmUp()
    {
        var (app, scheduler, interval, config) = BuildAppWithFakeScheduler();

        app.UseApiBackgroundJobs(includeMaintenanceJobs: false);

        scheduler.Received(1).Schedule<MeilisearchWarmUpJob>();
        interval.Received(1).Cron(Arg.Any<string>());
        config.Received(1).RunOnceAtStart();
        scheduler.DidNotReceive().Schedule<AccountMaintenanceJob>();
        scheduler.DidNotReceive().Schedule<AlbumRestockJob>();
    }

    [Fact]
    public void UseApiBackgroundJobs_MaintenanceJobsIncluded_SchedulesAllThreeJobs()
    {
        var (app, scheduler, _, _) = BuildAppWithFakeScheduler();

        app.UseApiBackgroundJobs(includeMaintenanceJobs: true);

        scheduler.Received(1).Schedule<MeilisearchWarmUpJob>();
        scheduler.Received(1).Schedule<AccountMaintenanceJob>();
        scheduler.Received(1).Schedule<AlbumRestockJob>();
    }

    private static (
        WebApplication App,
        IScheduler Scheduler,
        IScheduleInterval Interval,
        IScheduledEventConfiguration Config
    ) BuildAppWithFakeScheduler()
    {
        var scheduler = Substitute.For<IScheduler>();
        var interval = Substitute.For<IScheduleInterval>();
        var config = Substitute.For<IScheduledEventConfiguration>();

        scheduler.Schedule<MeilisearchWarmUpJob>().Returns(interval);
        scheduler.Schedule<AccountMaintenanceJob>().Returns(interval);
        scheduler.Schedule<AlbumRestockJob>().Returns(interval);
        interval.Cron(Arg.Any<string>()).Returns(config);
        config.RunOnceAtStart().Returns(config);

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(scheduler);
        builder.Services.AddSingleton(Options.Create(new MeilisearchWarmUpOptions()));
        builder.Services.AddSingleton(Options.Create(new AccountMaintenanceOptions()));
        builder.Services.AddSingleton(Options.Create(new AlbumRestockOptions()));

        return (builder.Build(), scheduler, interval, config);
    }
}
