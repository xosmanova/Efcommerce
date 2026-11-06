using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("NewsletterSubscribers");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(n => n.Source)
            .HasMaxLength(50);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.HasIndex(n => n.Email)
            .IsUnique();
    }
}
