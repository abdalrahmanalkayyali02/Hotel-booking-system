using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class PermissionsTranslationConfiguration : IEntityTypeConfiguration<PermissionsTranslation>
{
    public void Configure(EntityTypeBuilder<PermissionsTranslation> builder)
    {
        builder.ToTable("PermissionTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PermissionId)
            .IsRequired();

        builder
            .HasOne(permissionTranslation => permissionTranslation.Permission)
            .WithMany(permission => permission.PermissionTranslations)
            .HasForeignKey(permissionTranslation => permissionTranslation.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.LanguageId)
            .IsRequired();

        builder
            .HasOne(permissionTranslation => permissionTranslation.Language)
            .WithMany(language => language.PermissionTranslations)
            .HasForeignKey(permissionTranslation => permissionTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PermissionId, x.LanguageId })
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(x => x.Description)
            .HasMaxLength(255);
    }
}