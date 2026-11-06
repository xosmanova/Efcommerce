using ECommerce.Application.DTOs.Newsletter;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
public class NewsletterController : BaseApiController
{
    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    /// <summary>
    /// Subscribe an email to the weekly newsletter and receive a discount promo code.
    /// </summary>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeNewsletterDto dto, CancellationToken cancellationToken)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@') || !dto.Email.Contains('.'))
        {
            return Failure("Zəhmət olmasa düzgün e-poçt ünvanı daxil edin.");
        }

        var result = await _newsletterService.SubscribeAsync(dto, cancellationToken);
        return Success(result, result.Message);
    }

    /// <summary>
    /// Unsubscribe an email from the newsletter.
    /// </summary>
    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    public async Task<IActionResult> Unsubscribe([FromBody] SubscribeNewsletterDto dto, CancellationToken cancellationToken)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
        {
            return Failure("E-poçt ünvanı daxil edilməyib.");
        }

        var unsubscribed = await _newsletterService.UnsubscribeAsync(dto.Email, cancellationToken);
        if (!unsubscribed)
        {
            return Failure("Bu e-poçt ünvanı aktiv abunəçilər siyahısında tapılmadı.");
        }

        return Success("Abunəliyiniz uğurla ləğv edildi.");
    }

    /// <summary>
    /// Get all newsletter subscribers (Admin only).
    /// </summary>
    [HttpGet("subscribers")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetSubscribers(CancellationToken cancellationToken)
    {
        var subscribers = await _newsletterService.GetAllSubscribersAsync(cancellationToken);
        return Success(subscribers);
    }

    /// <summary>
    /// Get active subscriber count (Admin only).
    /// </summary>
    [HttpGet("count")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetCount(CancellationToken cancellationToken)
    {
        var count = await _newsletterService.GetSubscribersCountAsync(cancellationToken);
        return Success(new { count });
    }
}
