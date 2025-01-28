using Newsletter.Models;

namespace Newsletter.Services;

public interface INewsletterService
{
    ValidationResult EnlistSubscriber(Subscriber subscriber);
}
