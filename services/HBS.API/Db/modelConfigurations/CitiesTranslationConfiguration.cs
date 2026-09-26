using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class CitiesTranslationConfiguration : IEntityTypeConfiguration<CitiesTranslation>
{
    public void Configure(EntityTypeBuilder<CitiesTranslation> builder)
    {
        builder.ToTable("CitiesTranslations");
        builder.HasKey(cityTranslation => cityTranslation.Id);

        builder.HasOne(cityTranslation => cityTranslation.City)
            .WithMany(city => city.CitiesTranslations)
            .HasForeignKey(cityTranslation => cityTranslation.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cityTranslation => cityTranslation.Language)
            .WithMany(language => language.CityTranslations)
            .HasForeignKey(cityTranslation => cityTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(cityTranslation => cityTranslation.CityId)
            .IsRequired();

        builder.Property(cityTranslation => cityTranslation.LanguageId)
            .IsRequired();

        builder.HasIndex(cityTranslation => new { cityTranslation.LanguageId, cityTranslation.CityId })
            .IsUnique();

        builder.Property(cityTranslation => cityTranslation.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
