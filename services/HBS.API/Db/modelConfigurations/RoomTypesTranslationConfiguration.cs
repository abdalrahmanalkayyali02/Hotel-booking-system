using System.Security.Cryptography.X509Certificates;
using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RoomTypesTranslationConfiguration : IEntityTypeConfiguration<RoomTypesTranslation>
{
    public void Configure(EntityTypeBuilder<RoomTypesTranslation> builder)
    {
        builder.ToTable("RoomTypesTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoomTypeId)
            .IsRequired();
        
        builder.Property(x=>x.LanguageId)
            .IsRequired();
        
        builder.Property(x=>x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);
        
        builder.HasIndex(x=> new {x.RoomTypeId, x.LanguageId})
            .IsUnique();
        
        builder.HasOne(x=>x.RoomType)
            .WithMany(x=>x.RoomTypeTranslations)
            .HasForeignKey(x=>x.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
       
        builder.HasOne(x=>x.Language)
            .WithMany(x=>x.RoomTypeTranslations)
            .HasForeignKey(x=>x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}