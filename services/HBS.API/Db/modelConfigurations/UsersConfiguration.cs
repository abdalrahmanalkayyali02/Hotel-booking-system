using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class UsersConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => x.Email)
            .IsUnique();
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.BirthDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(x => x.IsEmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PhoneNumberCountryCode)
            .HasMaxLength(4);

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(9);

        builder.Property(x => x.PhoneNumberConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder
            .HasOne(user => user.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(user => user.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.VerifiedAt)
            .HasColumnType("datetime");

        builder.Property(x => x.CountryId)
            .IsRequired();

        builder
            .HasOne(user => user.Country)
            .WithMany(country => country.Users)
            .HasForeignKey(user => user.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CityId)
            .IsRequired();

        builder
            .HasOne(user => user.City)
            .WithMany(city => city.Users)
            .HasForeignKey(user => user.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}