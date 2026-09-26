using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RegionsTranslationConfiguration : IEntityTypeConfiguration<RegionsTranslation>
{
    public void Configure(EntityTypeBuilder<RegionsTranslation> builder)
    {
        builder.ToTable("RegionsTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegionId)
            .IsRequired();

        builder.Property(x => x.LanguageId)
            .IsRequired();

        builder.HasOne(x => x.Region)
            .WithMany(x => x.RegionsTranslations)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language)
            .WithMany(x => x.RegionTranslations)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RegionId, x.LanguageId })
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
