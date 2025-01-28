using Newsletter.Models;

namespace Newsletter.Services;

public interface INewsletterService
{
    Task<ValidationResult> EnlistSubscriberAsync(Subscriber subscriber);
}
