using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class BookingsConfiguration : IEntityTypeConfiguration<Bookings>
{
    public void Configure(EntityTypeBuilder<Bookings> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.BookingReference)
            .IsRequired();
        
        builder.HasIndex(booking=>booking.BookingReference)
            .IsUnique();

        builder.Property(booking => booking.UserId)
            .IsRequired();

        builder.Property(booking => booking.HotelId)
            .IsRequired();

        builder.Property(booking => booking.RoomTypeId)
            .IsRequired();

        builder.Property(booking => booking.RoomId)
            .IsRequired(false);

        builder.Property(booking => booking.CheckInDate)
            .IsRequired();

        builder.Property(booking => booking.CheckOutDate)
            .IsRequired();

        builder.Property(booking => booking.NumberOfGuests)
            .IsRequired();

        builder.Property(booking => booking.Status)
            .IsRequired();

        builder.Property(booking => booking.TotalAmount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(booking => booking.PromotionId);
        
        builder.HasOne(booking=>booking.Hotel)
            .WithMany(hotel=>hotel.Bookings)
            .HasForeignKey(booking=>booking.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(booking=>booking.RoomType)
            .WithMany(roomType=>roomType.Bookings)
            .HasForeignKey(booking=>booking.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(booking => booking.Room)
            .WithMany(room => room.Bookings)
            .HasForeignKey(booking => booking.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(booking=>booking.User)
            .WithMany(user=>user.Bookings)
            .HasForeignKey(booking=>booking.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
