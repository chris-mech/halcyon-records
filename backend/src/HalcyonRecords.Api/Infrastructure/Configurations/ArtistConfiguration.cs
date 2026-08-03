using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable(
            "Artists",
            t => t.HasCheckConstraint("CK_Artists_Name_NotEmpty", "LEN(Name) > 0")
        );

        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Name).HasMaxLength(150);
        builder.Property(a => a.Origin).HasMaxLength(200);
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);

        builder.Property(a => a.ImageUrl).HasMaxLength(500);
    }
}
