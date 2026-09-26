using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RoomTypesConfiguration : IEntityTypeConfiguration<RoomTypes>
{
    public void Configure(EntityTypeBuilder<RoomTypes> builder)
    {
        builder.ToTable("RoomTypes");
        builder.HasKey(roomType => roomType.Id);

        builder.Property(roomType => roomType.HotelId)
            .IsRequired();

        builder.Property(roomType => roomType.BasePrice)
            .IsRequired()
            .HasPrecision(10, 2);
        
        builder.Property(roomType=>roomType.MaxOccupancy)
            .IsRequired();
        
        builder.HasOne(roomTypes=>roomTypes.Hotel)
            .WithMany(hotels=>hotels.RoomTypes)
            .HasForeignKey(roomTypes=>roomTypes.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}