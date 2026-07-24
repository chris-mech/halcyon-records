using FluentAssertions;
using HalcyonRecords.Api.Infrastructure;

namespace HalcyonRecords.Api.UnitTests.Common;

public class EfCoreConventionTests
{
    [Fact]
    public void Model_ShouldNotHaveAnyEagerLoadedNavigations()
    {
        using var dbContext = new ApplicationDbContextFactory().CreateDbContext([]);

        var eagerLoadedNavigations = dbContext
            .Model.GetEntityTypes()
            .SelectMany(entityType => entityType.GetNavigations())
            .Where(navigation => navigation.IsEagerLoaded)
            .Select(navigation =>
                $"{navigation.DeclaringEntityType.ClrType.Name}.{navigation.Name}"
            )
            .ToList();

        eagerLoadedNavigations
            .Should()
            .BeEmpty(
                "no navigation should use .AutoInclude() - list-query slices must project explicitly via .Select() to avoid cartesian-product bugs"
            );
    }
}
