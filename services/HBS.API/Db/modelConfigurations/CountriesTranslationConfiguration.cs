using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class CountriesTranslationConfiguration : IEntityTypeConfiguration<CountriesTranslation>
{
    public void Configure(EntityTypeBuilder<CountriesTranslation> builder)
    {
        builder.ToTable("CountriesTranslations");
        builder.HasKey(countryTranslation => countryTranslation.Id);

        builder.Property(countryTranslation => countryTranslation.CountryId)
            .IsRequired();

        builder
            .HasOne(countriesTranslation => countriesTranslation.Country)
            .WithMany(country => country.CountryTranslations)
            .HasForeignKey(countriesTranslation => countriesTranslation.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(countryTranslation => countryTranslation.LanguageId)
            .IsRequired();

        builder
            .HasOne(countriesTranslation => countriesTranslation.Language)
            .WithMany(language => language.CountryTranslations)
            .HasForeignKey(countriesTranslation => countriesTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(countryTranslation => new { countryTranslation.CountryId, countryTranslation.LanguageId })
            .IsUnique();

        builder.Property(countryTranslation => countryTranslation.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}