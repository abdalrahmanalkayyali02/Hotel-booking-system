using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RoomTypeImagesConfiguration : IEntityTypeConfiguration<RoomTypeImages>
{
    public void Configure(EntityTypeBuilder<RoomTypeImages> builder)
    {
        builder.ToTable("RoomTypeImages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoomTypeId)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasOne(x => x.RoomType)
            .WithMany(x => x.RoomTypeImages)
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}
