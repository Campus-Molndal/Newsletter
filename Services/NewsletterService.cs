using System.Text.RegularExpressions;
using Newsletter.Data;
using Newsletter.Models;

namespace Newsletter.Services;

public class NewsletterService : INewsletterService
{
    private readonly ISubscriberRepository _subscriberRepository;

    public NewsletterService(ISubscriberRepository subscriberRepository)
    {
        _subscriberRepository = subscriberRepository;
    }

    public async Task<ValidationResult> EnlistSubscriberAsync(Subscriber subscriber)
    {
        // Validate email format
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(subscriber.Email ?? string.Empty))
        {
            return ValidationResult.Failure("Invalid email format.");
        }

        // Check for duplicate email
        if (await _subscriberRepository.GetSubscriberByEmailAsync(subscriber.Email!) != null)
        {
            return ValidationResult.Failure("This email is already registered.");
        }

        // Simulate adding the subscriber to the system
        await _subscriberRepository.AddSubscriberAsync(subscriber);
        return ValidationResult.Success();
    }
}