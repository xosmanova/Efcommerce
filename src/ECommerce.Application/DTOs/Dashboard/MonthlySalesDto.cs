namespace ECommerce.Application.DTOs.Dashboard;

public class MonthlySalesDto
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
