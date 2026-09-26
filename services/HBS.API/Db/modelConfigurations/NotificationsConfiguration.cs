using HBS.API.Db.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HBS.API.Db.modelConfigurations;

public class NotificationsConfiguration : IEntityTypeConfiguration<Notifications>
{
    public void Configure(EntityTypeBuilder<Notifications> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();
        
        builder.Property(x=>x.Title)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x=>x.Message)
            .IsRequired()
            .HasColumnType("text");
        
        builder.Property(x=>x.Type)
            .IsRequired()
            .HasMaxLength(40);
        
        builder.Property(x=>x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasOne(notification => notification.User)
            .WithMany(user => user.Notifications)
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}