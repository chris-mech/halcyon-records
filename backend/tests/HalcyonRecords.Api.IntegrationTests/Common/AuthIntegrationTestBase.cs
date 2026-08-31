using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.Auth;
using HalcyonRecords.Api.Infrastructure.Sql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace HalcyonRecords.Api.IntegrationTests.Common;

public abstract class AuthIntegrationTestBase : IntegrationTestBase
{
    protected AuthIntegrationTestBase(SqlServerContainerFixture fixture)
        : base(fixture)
    {
        UserManager = CreateUserManager(DbContext);
        JwtTokenService = new JwtTokenService(
            Options.Create(
                new JwtOptions
                {
                    SigningKey = "integration-test-signing-key-at-least-32-bytes-long",
                    Issuer = "HalcyonRecords.Api.Tests",
                    Audience = "HalcyonRecords.Api.Tests",
                }
            ),
            TimeProvider
        );
    }

    protected FakeTimeProvider TimeProvider { get; } = new();

    protected UserManager<User> UserManager { get; }

    protected JwtTokenService JwtTokenService { get; }

    private static UserManager<User> CreateUserManager(ApplicationDbContext dbContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services.BuildServiceProvider().GetRequiredService<UserManager<User>>();
    }
}
