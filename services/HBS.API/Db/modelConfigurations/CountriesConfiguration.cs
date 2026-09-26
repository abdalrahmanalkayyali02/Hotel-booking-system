using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class CountriesConfiguration : IEntityTypeConfiguration<Countries>
{
    public void Configure(EntityTypeBuilder<Countries> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(country => country.Id);

        builder.Property(country => country.CountryName)
            .IsRequired()
            .HasMaxLength(50);
    }
}