using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class StatesTranslationConfiguration : IEntityTypeConfiguration<StatesTranslation>
{
    public void Configure(EntityTypeBuilder<StatesTranslation> builder)
    {
        builder.ToTable("StatesTranslations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StateId)
            .IsRequired();

        builder
            .HasOne(stateTranslation => stateTranslation.State)
            .WithMany(states => states.StatesTranslations)
            .HasForeignKey(stateTranslation => stateTranslation.StateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.LanguageId)
            .IsRequired();

        builder
            .HasOne(stateTranslations => stateTranslations.Language)
            .WithMany(language => language.StateTranslations)
            .HasForeignKey(stateTranslation => stateTranslation.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LanguageId, x.StateId })
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}