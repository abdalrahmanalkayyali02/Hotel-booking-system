using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class HotelAmenitiesConfiguration : IEntityTypeConfiguration<HotelAmenities>
{
    public void Configure(EntityTypeBuilder<HotelAmenities> builder)
    {
        builder.ToTable("HotelAmenities");

        builder.HasKey(hotelAmenity => hotelAmenity.Id);

        builder.Property(hotelAmenity => hotelAmenity.HotelId)
            .IsRequired();

        builder.Property(hotelAmenity => hotelAmenity.AmenityId)
            .IsRequired();

        builder.HasIndex(hotelAmenity => new { hotelAmenity.HotelId, hotelAmenity.AmenityId })
            .IsUnique();

        builder.HasOne(hotelAmenity => hotelAmenity.Hotel)
            .WithMany(hotel => hotel.HotelAmenities)
            .HasForeignKey(hotelAmenity => hotelAmenity.HotelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(hotelAmenity => hotelAmenity.Amenity)
            .WithMany(amenity => amenity.HotelAmenities)
            .HasForeignKey(hotelAmenity => hotelAmenity.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}