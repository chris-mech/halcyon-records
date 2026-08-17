using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Domain.Ids;
using HalcyonRecords.Api.Infrastructure.Conversions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Decade> Decades => Set<Decade>();
    public DbSet<AlbumArtist> AlbumArtists => Set<AlbumArtist>();
    public DbSet<AlbumGenre> AlbumGenres => Set<AlbumGenre>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AlbumId>().HaveConversion<AlbumIdConverter>();
        configurationBuilder.Properties<ArtistId>().HaveConversion<ArtistIdConverter>();
        configurationBuilder.Properties<GenreId>().HaveConversion<GenreIdConverter>();
        configurationBuilder.Properties<CartId>().HaveConversion<CartIdConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
