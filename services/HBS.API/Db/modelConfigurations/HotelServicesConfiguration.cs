using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class HotelServicesConfiguration : IEntityTypeConfiguration<HotelServices>
{
    public void Configure(EntityTypeBuilder<HotelServices> builder)
    {
        builder.ToTable("HotelServices");
        builder.HasKey(hotelService => hotelService.Id);

        builder.Property(hotelService => hotelService.HotelId)
            .IsRequired();

        builder.Property(hotelService => hotelService.ServiceId)
            .IsRequired();

        builder.HasIndex(hotelService => new { hotelService.HotelId, hotelService.ServiceId })
            .IsUnique();
        
        builder.HasOne(hotelService=>hotelService.Services)
            .WithMany(service=>service.HotelServices)
            .HasForeignKey(hotelService=>hotelService.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(hotelService=>hotelService.Hotels)
            .WithMany(hotel=>hotel.HotelServices)
            .HasForeignKey(hotelService=>hotelService.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
