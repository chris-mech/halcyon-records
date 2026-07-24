using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HalcyonRecords.Api.Infrastructure.Configurations;

public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable(
            "Genres",
            t => t.HasCheckConstraint("CK_Genres_Name_NotEmpty", "LEN(Name) > 0")
        );

        builder.Property(g => g.Name).HasMaxLength(150);
        builder.HasIndex(g => g.Name).IsUnique();

        builder.Property(g => g.Slug).HasMaxLength(200);
        builder.HasIndex(g => g.Slug).IsUnique();
    }
}
