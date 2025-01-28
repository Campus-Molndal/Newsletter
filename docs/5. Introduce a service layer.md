+++
title = "5. Introduce a service layer"
weight = 5
date = 2025-01-28
draft = false
+++

## Goal

Enhance your ASP.NET MVC application by introducing a service layer that handle business logic. We will perform server-side validation and simulate to store the subscriber into a database. This approach separates the presentation layer, which handles user interaction, from the business logic layer, which enforces rules and processes core application use cases.

## Step-by-step Guide

### 1. Create a `ValidationResult` Class

The `ValidationResult` class will encapsulate the outcome of validation, including success status and error messages.

#### Steps:

1. Open the `Services` folder (create it if it doesn’t exist). Add a new file named `ValidationResult.cs`.

> Services/ValidationResult.cs

```csharp
namespace Newsletter.Services;

public class ValidationResult
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public ValidationResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public ValidationResult(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static ValidationResult Success() => new ValidationResult(true);

    public static ValidationResult Failure(params string[] errors) 
        => new ValidationResult(false, errors.ToList());
}
```

> Purpose: This class provides a structured way to return validation results from the service to the controller. It helps decouple the service logic from the controller, ensuring clear communication of validation success or failure.
> 
> The static methods Success and Failure improve readability and simplify usage. Success returns a ValidationResult instance with a true status and no errors, while Failure returns a ValidationResult with a false status and one or more error messages. These methods make it clear when and how validation results are being generated in the service layer.

### 2. Define the Service Interface

The service interface declares the contract for subscriber validation and subscription logic.

#### Steps:

1. In the `Services` folder, add a new file named `INewsletterService.cs`:

> Services/INewsletterService.cs

```csharp
using Newsletter.Models;

namespace Newsletter.Services;

public interface INewsletterService
{
    Task<ValidationResult> EnlistSubscriberAsync(Subscriber subscriber);
}
```

> **Purpose:** The interface defines the methods required for validating and enlisting subscribers, promoting a clean separation of concerns.

### 3. Implement the Newsletter Service

The implementation will include logic for validating email structure, checking for duplicates, and enlisting the subscriber.

#### Steps:

1. In the `Services` folder, add a new file named `NewsletterService.cs`:

> Services/NewsletterService.cs

```csharp
using System.Text.RegularExpressions;
using Newsletter.Models;

namespace Newsletter.Services;

public class NewsletterService : INewsletterService
{
    private readonly List<string> _registeredEmails = new List<string>(); // Mocked database

    public async Task<ValidationResult> EnlistSubscriberAsync(Subscriber subscriber)
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
        await Task.Delay(100); // Simulate an operation
        _registeredEmails.Add(subscriber.Email!);
        return ValidationResult.Success();
    }
}
```

> **Purpose:** The service contains business logic for email validation, preventing duplicates, and enlisting subscribers.


### 4. Register the Service in Dependency Injection

Register the `NewsletterService` for dependency injection so it can be used in the controller.

#### Steps:

1. Open `Program.cs`. Add the following line to register the service:

> Program.cs

```csharp
using Newsletter.Services;

...

builder.Services.AddScoped<INewsletterService, NewsletterService>();

...

```

> **Purpose:** Dependency injection allows the controller to use the service without creating tight coupling.


### 5. Update the Controller to Use the Service

The controller will delegate validation and subscription logic to the service and handle validation results.

#### Steps:

1. Open `NewsletterController.cs` in the `Controllers` folder. Modify the controller as follows:

> Controllers/NewsletterController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Newsletter.Models;
using Newsletter.Services;

namespace Newsletter.Controllers;

public class NewsletterController : Controller
{

    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    public IActionResult Index()
    {
        // Create a subscriber object and pass it to the view
        var subscriber = new Subscriber
        {
            Id = Guid.NewGuid().ToString(),
            Name = "John Doe",
            Email = "john.doe@email.com"
        };
        Console.WriteLine($"Subscriber {subscriber.Name}, {subscriber.Email} with ID: {subscriber.Id} created");
        return View(subscriber);
    }

    [HttpGet]
    public IActionResult Subscribe()
    {
        return View(new Subscriber());
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe(Subscriber subscriber)
    {
        // Validate the model state
        if (ModelState.IsValid)
        {
            // Log the subscriber details
            Console.WriteLine($"Name: {subscriber.Name}, Email: {subscriber.Email}");

            // TODO: Implement the subscription logic
            var result = await _newsletterService.EnlistSubscriberAsync(subscriber);

            if (!result.IsSuccess)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(subscriber); // Redisplay the form with validation errors
            }

            // Set a result message in TempData in order to display it in the view
            TempData["SuccessMessage"] = "You have successfully subscribed to our newsletter!";

            // Redirect back to the Subscribe GET action to clear the form
            return RedirectToAction("Subscribe");
        }

        // If the model state is invalid, redisplay the form with validation errors
        return View(subscriber);
    }

}
```

> **Purpose:** The controller focuses on flow control, while the service handles business logic.


### 6. Update the View

Ensure the view displays errors returned by the service.

#### Steps:

1. Open `Subscribe.cshtml` in the `Views/Newsletter` folder:

> Views/Newsletter/Subscribe.cshtml
2. Ensure the validation summary is included:

```html
@model Newsletter.Models.Subscriber

<!-- Display validation error message sent by the controller -->
@if (!ViewData.ModelState.IsValid)
{
    <div class="alert alert-danger alert-dismissible fade show" role="alert">
        @Html.ValidationSummary(false, null, new { @class = "text-danger" })
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"><i class="fas fa-times"></i></button>
    </div>
}

<!-- Display message from the TempData sent by the controller -->
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"><i class="fas fa-times"></i></button>
    </div>
}

<h2>Subscribe to our newsletter</h2>

<!-- The form to subscribe to the newsletter. Uses ASP tag helpers for validation -->
<form asp-action="Subscribe" method="post">
    <div class="form-group">
        <label asp-for="Name" class="control-label"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label asp-for="Email" class="control-label"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Subscribe</button>
</form>

<!-- This script is used to display the validation error messages on the client side -->
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

> **Purpose:** The validation summary and field-specific errors will display messages from the service.


## Summary

- **ValidationResult**: Structured validation result to communicate errors.
- **Service Interface**: Defined contract for the business logic.
- **Service Implementation**: Encapsulated email validation and duplicate checks.
- **Dependency Injection**: Registered the service for controller use.
- **Controller Update**: Delegated validation and subscription logic to the service.
- **View Update**: Displayed validation messages effectively.

Your application now has service layer. 🎉

