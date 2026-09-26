using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class HotelRequestsConfiguration : IEntityTypeConfiguration<HotelRequests>
{
    public void Configure(EntityTypeBuilder<HotelRequests> builder)
    {
        builder.ToTable("HotelRequests");

        builder.HasKey(hotelRequest => hotelRequest.Id);

        builder.Property(hotelRequest => hotelRequest.UserId)
            .IsRequired();

        builder.Property(hotelRequest => hotelRequest.HotelId)
            .IsRequired();

        builder.Property(hotelRequest => hotelRequest.Status)
            .IsRequired();

        builder.Property(hotelRequest => hotelRequest.RejectionReason)
            .HasMaxLength(500);

        builder.Property(hotelRequest => hotelRequest.ReviewedAt);

        builder.Property(hotelRequest => hotelRequest.ReviewedBy);

        builder.HasOne(hotelRequest => hotelRequest.User)
            .WithMany(user => user.HotelRequests)
            .HasForeignKey(hotelRequest => hotelRequest.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(hotelRequest => hotelRequest.Hotel)
            .WithMany(hotel => hotel.HotelRequests)
            .HasForeignKey(hotelRequest => hotelRequest.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}