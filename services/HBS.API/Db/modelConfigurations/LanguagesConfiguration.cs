using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class LanguagesConfiguration : IEntityTypeConfiguration<Languages>
{
    public void Configure(EntityTypeBuilder<Languages> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(language => language.Id);

        builder.Property(language => language.LanguageName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(language => language.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(language => language.Code)
            .IsUnique();
    }
}