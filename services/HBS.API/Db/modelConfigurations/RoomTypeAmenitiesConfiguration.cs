using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace HBS.API.Db.modelConfigurations;

public class RoomTypeAmenitiesConfiguration : IEntityTypeConfiguration<RoomTypeAmenities>
{
    public void Configure(EntityTypeBuilder<RoomTypeAmenities> builder)
    {
        builder.ToTable("RoomTypeAmenities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoomTypeId)
            .IsRequired();
        
        builder.Property(x => x.AmenityId)
            .IsRequired();
        
        builder.HasIndex(x => new { x.RoomTypeId, x.AmenityId })
            .IsUnique();
        
        builder.HasOne(x=>x.RoomType)
            .WithMany(x=>x.RoomTypeAmenities)
            .HasForeignKey(x=>x.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=>x.Amenity)
            .WithMany(x=>x.RoomTypeAmenities)
            .HasForeignKey(x=>x.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);
            
            
    }
}