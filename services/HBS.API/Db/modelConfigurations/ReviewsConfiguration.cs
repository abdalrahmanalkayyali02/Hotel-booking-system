using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class ReviewsConfiguration : IEntityTypeConfiguration<Reviews>
{
    public void Configure(EntityTypeBuilder<Reviews> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.HotelId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.BookingId);

        builder.HasIndex(x=>x.BookingId)
            .IsUnique();
        
        builder.Property(x => x.Rating)
            .IsRequired();

        builder.Property(x => x.Comment)
            .IsRequired(false);
        
        builder.HasOne(x=>x.Hotel)
            .WithMany(x=>x.Reviews)
            .HasForeignKey(x=>x.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=>x.User)
            .WithMany(x=>x.Reviews)
            .HasForeignKey(x=>x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=>x.Booking)
            .WithOne(x=>x.Review)
            .HasForeignKey<Reviews>(x=>x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
