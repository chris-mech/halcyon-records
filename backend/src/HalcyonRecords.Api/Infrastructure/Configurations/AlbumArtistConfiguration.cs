using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class AlbumArtistConfiguration : IEntityTypeConfiguration<AlbumArtist>
{
    public void Configure(EntityTypeBuilder<AlbumArtist> builder)
    {
        builder.HasKey(aa => new { aa.AlbumId, aa.ArtistId });

        builder
            .HasOne(aa => aa.Album)
            .WithMany(a => a.AlbumArtists)
            .HasForeignKey(aa => aa.AlbumId);

        builder
            .HasOne(aa => aa.Artist)
            .WithMany(a => a.AlbumArtists)
            .HasForeignKey(aa => aa.ArtistId);
    }
}
