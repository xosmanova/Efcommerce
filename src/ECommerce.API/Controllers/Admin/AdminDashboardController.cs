using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Dashboard;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminDashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public AdminDashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Returns live dashboard statistics: total revenue, revenue change percentage,
    /// total orders, pending orders, total active products, low stock count, and out of stock count.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats(
        [FromQuery] int lowStockThreshold = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetStatsAsync(lowStockThreshold, cancellationToken);
        return Success(result, "Dashboard statistics retrieved successfully.");
    }

    /// <summary>
    /// Returns monthly sales revenue and order counts for the last specified number of months.
    /// </summary>
    [HttpGet("sales")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MonthlySalesDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSales(
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetSalesAsync(months, cancellationToken);
        return Success(result, "Dashboard sales retrieved successfully.");
    }

    /// <summary>
    /// Returns the latest live activities (orders, payments, low stock warnings, new user registrations).
    /// </summary>
    [HttpGet("activities")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DashboardActivityDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivities(
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetActivitiesAsync(take, cancellationToken);
        return Success(result, "Dashboard activities retrieved successfully.");
    }
}
