using HBS.API.Db.models;
using HBS.API.Shared.enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RoomsConfiguration : IEntityTypeConfiguration<Rooms>
{
    public void Configure(EntityTypeBuilder<Rooms> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(rooms => rooms.Id);

        builder.Property(rooms => rooms.HotelId)
            .IsRequired();
        
        builder.Property(rooms=>rooms.RoomTypeId)
            .IsRequired();
        
        builder.Property(rooms=>rooms.RoomNumber)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(rooms => rooms.Floor);
        
        builder.Property(rooms=>rooms.Status)
            .IsRequired()
            .HasDefaultValue(RoomStatus.Available);
        
        builder.HasOne(room=>room.Hotel)
            .WithMany(hotel => hotel.Rooms)
            .HasForeignKey(room => room.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(room => room.RoomType)
            .WithMany(roomType => roomType.Rooms)
            .HasForeignKey(room => room.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}