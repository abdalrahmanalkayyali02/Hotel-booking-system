using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RegionsConfiguration : IEntityTypeConfiguration<Regions>
{
    public void Configure(EntityTypeBuilder<Regions> builder)
    {
        builder.ToTable("Regions");
        builder.HasKey(r => r.Id);

        builder.Property(x => x.RegionName)
            .IsRequired()
            .HasMaxLength(50);
    }
}