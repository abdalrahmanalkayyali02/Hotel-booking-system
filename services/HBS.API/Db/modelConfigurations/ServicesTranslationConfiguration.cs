using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class ServicesTranslationConfiguration : IEntityTypeConfiguration<ServicesTranslation>
{
    public void Configure(EntityTypeBuilder<ServicesTranslation> builder)
    {
        builder.ToTable("ServicesTranslation");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ServiceId)
            .IsRequired();
        
        builder.Property(x => x.LanguageId)
            .IsRequired();
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);
        
        builder.HasIndex(x=> new  { x.LanguageId, x.Name })
            .IsUnique();
        
        builder.HasOne(x=>x.Service)
            .WithMany(x=>x.ServicesTranslations)
            .HasForeignKey(x=>x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=>x.Language)
            .WithMany(x=>x.ServicesTranslations)
            .HasForeignKey(x=>x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}