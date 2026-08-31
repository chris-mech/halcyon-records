using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Infrastructure.Sql;

public static class SqlServerExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddApiSqlServer()
        {
            builder.AddSqlServerDbContext<ApplicationDbContext>(
                "halcyonrecords",
                configureSettings: settings => settings.DisableRetry = true,
                configureDbContextOptions: options =>
                    options.UseSqlServer(sql =>
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(15),
                            errorNumbersToAdd: null
                        )
                    )
            );

            return builder;
        }
    }
}
