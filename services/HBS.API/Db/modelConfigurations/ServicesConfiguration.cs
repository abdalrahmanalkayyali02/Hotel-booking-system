using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class ServicesConfiguration : IEntityTypeConfiguration<HBS.API.Db.models.Services>
{
    public void Configure(EntityTypeBuilder<HBS.API.Db.models.Services> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(x => x.Id);
        
        
        builder.Property(x=>x.Icon)
            .IsRequired()
            .HasMaxLength(255);
        
    }
}
