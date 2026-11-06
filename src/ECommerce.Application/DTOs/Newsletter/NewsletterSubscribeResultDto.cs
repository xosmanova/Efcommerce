namespace ECommerce.Application.DTOs.Newsletter;

public class NewsletterSubscribeResultDto
{
    public string Email { get; set; } = string.Empty;
    public string PromoCode { get; set; } = "WELCOME10";
    public decimal DiscountPercent { get; set; } = 10m;
    public bool IsNewSubscriber { get; set; }
    public string Message { get; set; } = string.Empty;
}
