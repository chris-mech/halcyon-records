using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure.Conversions;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Decade> Decades => Set<Decade>();
    public DbSet<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public DbSet<AlbumGenre> AlbumGenres => Set<AlbumGenre>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AlbumId>().HaveConversion<AlbumIdConverter>();
        configurationBuilder.Properties<ArtistId>().HaveConversion<ArtistIdConverter>();
        configurationBuilder.Properties<GenreId>().HaveConversion<GenreIdConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
