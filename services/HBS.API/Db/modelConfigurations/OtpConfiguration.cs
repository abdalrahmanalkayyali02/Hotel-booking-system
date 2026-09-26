using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class OtpConfiguration : IEntityTypeConfiguration<Otp>
{
    public void Configure(EntityTypeBuilder<Otp> builder)
    {
        builder.ToTable("Otp");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Otps)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.HashedOtp)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Target)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.GeneratedAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(x => x.ExpiresAt)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(x => x.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.NumberOfAttempts)
            .IsRequired()
            .HasDefaultValue(0);

    }
}
