using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public string Type { get; set; } = string.Empty; // "order", "payment", "stock", "user"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
