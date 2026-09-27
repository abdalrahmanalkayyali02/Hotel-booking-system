using HBS.API.Db.models;
using HBS.API.Shared.enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class HotelsConfiguration : IEntityTypeConfiguration<Hotels>
{
    public void Configure(EntityTypeBuilder<Hotels> builder)
    {
        builder.ToTable("Hotels");
        builder.HasKey(hotel=>hotel.Id);

        builder.Property(hotel => hotel.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(hotel => hotel.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(hotel => hotel.StarRating)
            .IsRequired();

        builder.Property(hotel => hotel.PhoneNumberCountryCode)
            .HasMaxLength(4);

        builder.Property(hotel=>hotel.PhoneNumber)
            .HasMaxLength(9);

        builder.Property(hotel=>hotel.Email)
            .HasMaxLength(255);

        builder.Property(hotel => hotel.Status)
          .IsRequired()
          .HasDefaultValue(HotelStatus.Pending);

        builder.HasOne(hotel=>hotel.Manager)
            .WithMany(user=>user.ManagedHotels)
            .HasForeignKey(hotel=>hotel.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
