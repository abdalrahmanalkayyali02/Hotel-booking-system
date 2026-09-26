using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class AmenitiesTranslationConfiguration : IEntityTypeConfiguration<AmenitiesTranslation>
{
    public void Configure(EntityTypeBuilder<AmenitiesTranslation> builder)
    {
        builder.ToTable("AmenitiesTranslation");
        builder.HasKey(amenityTranslation => amenityTranslation.Id);

        builder.Property(amenityTranslation => amenityTranslation.AmenityId)
            .IsRequired();
        
        builder.Property(amenityTranslation => amenityTranslation.LanguageId)
            .IsRequired();
        
        builder.Property(amenityTranslation => amenityTranslation.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(amenityTranslation => amenityTranslation.Description)
            .HasMaxLength(500);
        
        builder.HasIndex(amenityTranslation=> new {amenityTranslation.AmenityId, amenityTranslation.LanguageId})
            .IsUnique();
        
        builder.HasOne(amenityTranslation=>amenityTranslation.Amenity)
            .WithMany(amenity=>amenity.AmenityTranslations)
            .HasForeignKey(amenityTranslation=>amenityTranslation.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(amenityTranslation=>amenityTranslation.Language)
            .WithMany(language=>language.AmenityTranslations)
            .HasForeignKey(amenityTranslation=>amenityTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}