using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class AlbumGenreConfiguration : IEntityTypeConfiguration<AlbumGenre>
{
    public void Configure(EntityTypeBuilder<AlbumGenre> builder)
    {
        builder.HasKey(ag => new { ag.AlbumId, ag.GenreId });

        builder.HasOne(ag => ag.Album).WithMany(a => a.AlbumGenres).HasForeignKey(ag => ag.AlbumId);

        builder.HasOne(ag => ag.Genre).WithMany(g => g.AlbumGenres).HasForeignKey(ag => ag.GenreId);
    }
}
