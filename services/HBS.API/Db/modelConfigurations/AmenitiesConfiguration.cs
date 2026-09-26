using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class AmenitiesConfiguration : IEntityTypeConfiguration<Amenities>
{
    public void Configure(EntityTypeBuilder<Amenities> builder)
    {
        builder.ToTable("Amenities");
        builder.HasKey(amenities => amenities.Id);
        
        builder.Property(amenity=>amenity.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(amenity=>amenity.Name)
            .IsUnique();

        builder.Property(amenity => amenity.Icon)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(amenity => amenity.Description)
            .HasMaxLength(255);
    }
}