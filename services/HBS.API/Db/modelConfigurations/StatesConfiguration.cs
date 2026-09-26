using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class StatesConfiguration : IEntityTypeConfiguration<States>
{
    public void Configure(EntityTypeBuilder<States> builder)
    {
        builder.ToTable("States");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.States)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CountryId)
            .IsRequired();

        builder.Property(x => x.StateName)
            .IsRequired()
            .HasMaxLength(50);

    }
}
