using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class HotelsTranslationConfiguration : IEntityTypeConfiguration<HotelsTranslation>
{
    public void Configure(EntityTypeBuilder<HotelsTranslation> builder)
    {
        builder.ToTable("HotelsTranslations");
        builder.HasKey(hotelTranslation => hotelTranslation.Id);

        builder.Property(hotelTranslation => hotelTranslation.HotelId)
            .IsRequired();
        
        builder.Property(hotelTranslation=>hotelTranslation.LanguageId)
            .IsRequired();
        
        builder.Property(hotelTranslation=>hotelTranslation.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(hotelTranslation=>hotelTranslation.Description)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(hotelTranslation=>hotelTranslation.Address)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(hotelTranslation=> new {hotelTranslation.HotelId, hotelTranslation.LanguageId})
            .IsUnique();
        
        builder.HasOne(hotelTranslation=>hotelTranslation.Hotels)
            .WithMany(hotel=>hotel.Translations)
            .HasForeignKey(hotelTranslation=>hotelTranslation.HotelId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(hotelTranslation=>hotelTranslation.Languages)
            .WithMany(language=>language.HotelTranslations)
            .HasForeignKey(hotelTranslation=>hotelTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}