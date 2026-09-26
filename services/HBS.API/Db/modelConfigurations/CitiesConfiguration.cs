using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class CitiesConfiguration : IEntityTypeConfiguration<Cities>
{
    public void Configure(EntityTypeBuilder<Cities> builder)
    {
        builder.ToTable("Cities");
        builder.HasKey(city => city.Id);

        builder.HasOne(city => city.Country)
            .WithMany(country => country.Cities)
            .HasForeignKey(city => city.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(city => city.State)
            .WithMany(state => state.Cities)
            .HasForeignKey(city => city.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(city => city.Region)
            .WithMany(region => region.Cities)
            .HasForeignKey(city => city.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(city => city.CountryId);

        builder.Property(city => city.RegionId);

        builder.Property(city => city.StateId);

        builder.Property(city => city.CityName)
            .IsRequired()
            .HasMaxLength(100);

    }
}
