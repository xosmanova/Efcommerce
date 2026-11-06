namespace ECommerce.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public double? RevenueChangePercent { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}
