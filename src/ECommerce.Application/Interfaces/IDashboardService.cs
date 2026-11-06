using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int lowStockThreshold = 5, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlySalesDto>> GetSalesAsync(int months = 6, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardActivityDto>> GetActivitiesAsync(int take = 10, CancellationToken cancellationToken = default);
    Task LogActivityAsync(string type, string title, string description, CancellationToken cancellationToken = default);
}
