using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class SubRegionsTranslationConfiguration : IEntityTypeConfiguration<SubRegionsTranslation>
{
    public void Configure(EntityTypeBuilder<SubRegionsTranslation> builder)
    {
        builder.ToTable("SubRegionsTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubRegionId)
            .IsRequired();

        builder.Property(x => x.LanguageId)
            .IsRequired();

        builder.HasOne(x => x.SubRegion)
            .WithMany(x => x.SubRegionsTranslations)
            .HasForeignKey(x => x.SubRegionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language)
            .WithMany(x => x.SubRegionsTranslations)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SubRegionId, x.LanguageId })
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
