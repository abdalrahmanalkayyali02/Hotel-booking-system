using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class FavoritesConfiguration : IEntityTypeConfiguration<Favorites>
{
    public void Configure(EntityTypeBuilder<Favorites> builder)
    {
        builder.ToTable("Favorites");
        builder.HasKey(favorite => favorite.Id);

        builder.Property(favorite => favorite.UserId)
            .IsRequired();

        builder.Property(favorite => favorite.HotelId)
            .IsRequired();

        builder.HasIndex(favorite => new { favorite.UserId, favorite.HotelId })
            .IsUnique();
        
        builder.HasOne(favorite=>favorite.Users)
            .WithMany(user=>user.Favorites)
            .HasForeignKey(favorite=>favorite.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(favorite=>favorite.Hotels)
            .WithMany(hotel=>hotel.Favorites)
            .HasForeignKey(favorite=>favorite.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
        
            
    }
}
