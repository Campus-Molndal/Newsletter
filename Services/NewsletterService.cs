using System.Text.RegularExpressions;
using Newsletter.Models;

namespace Newsletter.Services;

public class NewsletterService : INewsletterService
{
    private readonly List<string> _registeredEmails = new List<string>(); // Mocked database

    public ValidationResult EnlistSubscriber(Subscriber subscriber)
    {
        // Validate email format
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(subscriber.Email ?? string.Empty))
        {
            return ValidationResult.Failure("Invalid email format.");
        }

        // Check for duplicate email
        if (_registeredEmails.Contains(subscriber.Email!))
        {
            return ValidationResult.Failure("This email is already registered.");
        }

        // Simulate adding the subscriber to the system
        _registeredEmails.Add(subscriber.Email!);
        return ValidationResult.Success();
    }
}