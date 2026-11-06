using ECommerce.Application.DTOs.Newsletter;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class NewsletterService : INewsletterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDashboardService _dashboardService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        IUnitOfWork unitOfWork,
        IDashboardService dashboardService,
        IEmailService emailService,
        ILogger<NewsletterService> logger)
    {
        _unitOfWork = unitOfWork;
        _dashboardService = dashboardService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<NewsletterSubscribeResultDto> SubscribeAsync(SubscribeNewsletterDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email?.Trim().ToLowerInvariant() ?? string.Empty;

        var existing = await _unitOfWork.Repository<NewsletterSubscriber>()
            .Query()
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail, cancellationToken);

        if (existing != null)
        {
            if (existing.IsActive)
            {
                return new NewsletterSubscribeResultDto
                {
                    Email = normalizedEmail,
                    PromoCode = "WELCOME10",
                    DiscountPercent = 10m,
                    IsNewSubscriber = false,
                    Message = "Siz artıq bülletenimizə abunəsiniz! Xüsusi promo-kodunuz: WELCOME10"
                };
            }

            // Re-activate if was previously unsubscribed
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await LogAndSendEmailAsync(normalizedEmail, isReactivated: true, cancellationToken);

            return new NewsletterSubscribeResultDto
            {
                Email = normalizedEmail,
                PromoCode = "WELCOME10",
                DiscountPercent = 10m,
                IsNewSubscriber = true,
                Message = "🎉 Abunəliyiniz uğurla bərpa edildi! 10% endirim promo-kodunuz: WELCOME10"
            };
        }

        // New subscriber
        var subscriber = new NewsletterSubscriber
        {
            Email = normalizedEmail,
            IsActive = true,
            Source = "Footer",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<NewsletterSubscriber>().AddAsync(subscriber, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await LogAndSendEmailAsync(normalizedEmail, isReactivated: false, cancellationToken);

        return new NewsletterSubscribeResultDto
        {
            Email = normalizedEmail,
            PromoCode = "WELCOME10",
            DiscountPercent = 10m,
            IsNewSubscriber = true,
            Message = "🎉 Təbriklər! Bülletenə uğurla abunə oldunuz! 10% endirim promo-kodunuz: WELCOME10"
        };
    }

    public async Task<bool> UnsubscribeAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;

        var existing = await _unitOfWork.Repository<NewsletterSubscriber>()
            .Query()
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail, cancellationToken);

        if (existing == null || !existing.IsActive)
        {
            return false;
        }

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await _dashboardService.LogActivityAsync("newsletter", "Bülleten abunəliyi dayandırıldı", $"{normalizedEmail} abunəlikdən çıxdı.", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log newsletter unsubscribe activity.");
        }

        return true;
    }

    public async Task<IReadOnlyList<NewsletterSubscriberDto>> GetAllSubscribersAsync(CancellationToken cancellationToken = default)
    {
        var subscribers = await _unitOfWork.Repository<NewsletterSubscriber>()
            .Query()
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new NewsletterSubscriberDto
            {
                Id = s.Id,
                Email = s.Email,
                IsActive = s.IsActive,
                Source = s.Source,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return subscribers;
    }

    public async Task<int> GetSubscribersCountAsync(CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<NewsletterSubscriber>()
            .Query()
            .AsNoTracking()
            .CountAsync(s => s.IsActive, cancellationToken);
    }

    private async Task LogAndSendEmailAsync(string email, bool isReactivated, CancellationToken cancellationToken)
    {
        // 1. Log activity to database for Admin Dashboard
        try
        {
            var title = isReactivated ? "Bülleten abunəliyi yeniləndi" : "Yeni bülleten abunəliyi";
            var desc = $"{email} xüsusi endirimlər bülleteninə abunə oldu.";
            await _dashboardService.LogActivityAsync("newsletter", title, desc, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log activity for newsletter subscriber {Email}", email);
        }

        // 2. Send welcome email with promo-code WELCOME10
        try
        {
            var subject = "ShopHub Xüsusi Endirim Promo-Kodunuz: WELCOME10";
            var htmlBody = $@"
<div style=""background-color: #f8fafc; font-family: 'Segoe UI', Arial, sans-serif; padding: 40px 16px;"">
    <div style=""max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 16px rgba(0,0,0,0.06); border: 1px solid #e2e8f0;"">
        <div style=""background: linear-gradient(135deg, #0ea5e9, #0284c7); padding: 32px 24px; text-align: center; color: #ffffff;"">
            <h1 style=""margin: 0; font-size: 28px; font-weight: 800; letter-spacing: -0.5px;"">Shop<span style=""color: #38bdf8;"">Hub</span></h1>
            <p style=""margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;"">Xüsusi Endirimlər və Kampaniyalar</p>
        </div>
        <div style=""padding: 32px 28px; color: #1e293b;"">
            <h2 style=""margin-top: 0; font-size: 20px; font-weight: 700; color: #0f172a; text-align: center;"">🎉 Abunəliyiniz uğurla tamamlandı!</h2>
            <p style=""font-size: 15px; line-height: 1.6; color: #475569; text-align: center;"">
                ShopHub bülleteninə abunə olduğunuz üçün təşəkkür edirik. Hər həftə yeni təkliflər, aksiyalar və promo-kodlar birbaşa sizin bu ünvana göndəriləcək:
            </p>
            <div style=""background: #f1f5f9; padding: 12px; border-radius: 8px; text-align: center; font-weight: 600; color: #334155; margin: 16px 0;"">
                {email}
            </div>
            <div style=""margin: 28px 0; text-align: center; background: #f0fdf4; border: 2px dashed #22c55e; border-radius: 14px; padding: 24px;"">
                <span style=""font-size: 13px; font-weight: 700; color: #15803d; text-transform: uppercase; letter-spacing: 1px;"">İlk Alış-verişinizə Xüsusi 10% Endirim Kuponu</span>
                <div style=""margin: 12px 0; font-size: 26px; font-weight: 900; color: #166534; letter-spacing: 3px;"">
                    WELCOME10
                </div>
                <p style=""margin: 0; font-size: 12px; color: #16a34a;"">
                    Səbətdə ödənişə keçərkən bu kuponu daxil edərək dərhal 10% endirimdən yararlana bilərsiniz.
                </p>
            </div>
            <div style=""text-align: center; margin-top: 24px;"">
                <a href=""http://localhost:5078/"" style=""display: inline-block; background: #0ea5e9; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 10px; font-weight: 700; font-size: 14px; box-shadow: 0 4px 12px rgba(14,165,233,0.3);"">
                    Mağazaya Keç və Alış-verişə Başla
                </a>
            </div>
        </div>
        <div style=""background: #f8fafc; border-top: 1px solid #f1f5f9; padding: 18px 24px; text-align: center; font-size: 11px; color: #94a3b8;"">
            © 2026 ShopHub E-Commerce. Bütün hüquqlar qorunur.<br>
            Spam göndərmirik. İstədiyiniz vaxt abunəlikdən imtina edə bilərsiniz.
        </div>
    </div>
</div>";

            await _emailService.SendEmailAsync(email, subject, htmlBody, cancellationToken);
            _logger.LogInformation("Newsletter welcome email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send newsletter welcome email to {Email}", email);
        }
    }
}
