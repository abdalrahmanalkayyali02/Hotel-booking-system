using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class HotelImagesConfigurations : IEntityTypeConfiguration<HotelImages>
{
    public void Configure(EntityTypeBuilder<HotelImages> builder)
    {
        builder.ToTable("HotelImages");
        builder.HasKey(hotelImage => hotelImage.Id);

        builder.Property(hotelImage => hotelImage.HotelId)
            .IsRequired();
        
        builder.Property(hotelImage=>hotelImage.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(hotelImage=>hotelImage.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasOne(hotelImage=>hotelImage.Hotel)
            .WithMany(hotel=>hotel.HotelImages)
            .HasForeignKey(hotelImage=>hotelImage.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}