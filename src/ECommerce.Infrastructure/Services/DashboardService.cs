using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    private static readonly string[] AzMonthNames =
    {
        "", "Yan", "Fev", "Mar", "Apr", "May", "İyn", "İyl", "Avq", "Sen", "Okt", "Noy", "Dek"
    };

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(int lowStockThreshold = 5, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = currentMonthStart.AddMonths(-1);
        var lastMonthEnd = currentMonthStart.AddTicks(-1);

        var totalRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var currentMonthRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= currentMonthStart && o.CreatedAt <= now)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        var lastMonthRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= lastMonthStart && o.CreatedAt <= lastMonthEnd)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        double? revenueChangePercent = null;
        if (lastMonthRevenue > 0)
        {
            revenueChangePercent = Math.Round((double)((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100.0, 1);
        }

        var totalOrders = await _context.Orders
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var pendingOrders = await _context.Orders
            .AsNoTracking()
            .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive, cancellationToken);

        var lowStockCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive && p.StockQuantity <= lowStockThreshold, cancellationToken);

        var outOfStockCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive && p.StockQuantity <= 0, cancellationToken);

        return new DashboardStatsDto
        {
            TotalRevenue = totalRevenue,
            RevenueChangePercent = revenueChangePercent,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            TotalProducts = totalProducts,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount
        };
    }

    public async Task<IReadOnlyList<MonthlySalesDto>> GetSalesAsync(int months = 6, CancellationToken cancellationToken = default)
    {
        var takeMonths = Math.Clamp(months, 1, 24);
        var now = DateTime.UtcNow;

        var startTargetMonth = now.AddMonths(-(takeMonths - 1));
        var rangeStart = new DateTime(startTargetMonth.Year, startTargetMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1);

        var ordersInPeriod = await _context.Orders
            .AsNoTracking()
            .Where(o => o.Status != OrderStatus.Cancelled && o.CreatedAt >= rangeStart && o.CreatedAt <= rangeEnd)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var ordersDict = ordersInPeriod.ToDictionary(
            x => (x.Year, x.Month),
            x => (x.Revenue, x.OrderCount));

        var result = new List<MonthlySalesDto>();

        for (int i = takeMonths - 1; i >= 0; i--)
        {
            var target = now.AddMonths(-i);
            var year = target.Year;
            var month = target.Month;
            var monthName = (month >= 1 && month <= 12) ? AzMonthNames[month] : target.ToString("MMM");

            decimal revenue = 0m;
            int orderCount = 0;

            if (ordersDict.TryGetValue((year, month), out var stats))
            {
                revenue = stats.Revenue;
                orderCount = stats.OrderCount;
            }

            result.Add(new MonthlySalesDto
            {
                Month = monthName,
                Year = year,
                Revenue = revenue,
                OrderCount = orderCount
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<DashboardActivityDto>> GetActivitiesAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var limit = Math.Clamp(take, 1, 50);

        var activities = await _context.ActivityLogs
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new DashboardActivityDto
            {
                Id = a.Id,
                Type = a.Type,
                Title = a.Title,
                Description = a.Description,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return activities;
    }

    public async Task LogActivityAsync(string type, string title, string description, CancellationToken cancellationToken = default)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            Type = type,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
