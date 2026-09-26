using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class RolesTranslationConfiguration : IEntityTypeConfiguration<RoleTranslation>
{
    public void Configure(EntityTypeBuilder<RoleTranslation> builder)
    {
        builder.ToTable("RoleTranslations");
        builder.HasKey(r => r.Id);

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder
            .HasOne(roleTranslation => roleTranslation.Role)
            .WithMany(role => role.RoleTranslations)
            .HasForeignKey(roleTranslation => roleTranslation.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.LanguageId)
            .IsRequired();

        builder
            .HasOne(roleTranslation => roleTranslation.Language)
            .WithMany(language => language.RoleTranslations)
            .HasForeignKey(roleTranslation => roleTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RoleId, x.LanguageId })
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(255);
    }
}