using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Source { get; set; } = "Footer";
}
