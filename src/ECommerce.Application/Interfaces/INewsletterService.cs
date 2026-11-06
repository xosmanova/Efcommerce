using ECommerce.Application.DTOs.Newsletter;

namespace ECommerce.Application.Interfaces;

public interface INewsletterService
{
    Task<NewsletterSubscribeResultDto> SubscribeAsync(SubscribeNewsletterDto dto, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsletterSubscriberDto>> GetAllSubscribersAsync(CancellationToken cancellationToken = default);
    Task<int> GetSubscribersCountAsync(CancellationToken cancellationToken = default);
}
