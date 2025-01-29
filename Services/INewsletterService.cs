using Newsletter.Models;

namespace Newsletter.Services;

public interface INewsletterService
{
    Task<ValidationResult> EnlistSubscriberAsync(Subscriber subscriber);
    Task<IEnumerable<Subscriber>> GetSubscribersAsync();
    Task<ValidationResult> CancelSubscriptionAsync(string subscriberEmail);
}