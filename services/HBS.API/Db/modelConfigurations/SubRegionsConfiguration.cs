using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class SubRegionsConfiguration : IEntityTypeConfiguration<SubRegions>
{
    public void Configure(EntityTypeBuilder<SubRegions> builder)
    {
        builder.ToTable("SubRegions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegionId)
            .IsRequired();

        builder.HasOne(x => x.Region)
            .WithMany(x => x.SubRegions)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
