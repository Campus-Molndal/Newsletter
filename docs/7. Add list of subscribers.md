+++
title = "List Subscribers with Access Control and Unsubscribe Option"
weight = 7
date = 2025-01-28
draft = false
+++

## Goal

In this tutorial, you will learn how to:

1. List all subscribers on a protected page.
2. Restrict access to the subscriber list so only logged-in users can view it.
3. Add an unsubscribe button for each subscriber in the list.
4. Display a success message when a user is successfully unsubscribed.
5. Add a **Subscribers** link in the navigation bar that toggles visibility based on the login status.

## Step-by-step Guide

### 1. Update the Newsletter Service

We need to add methods to fetch all subscribers and to unsubscribe a specific subscriber by their ID. These methods will ensure that the service can support the list and unsubscribe functionalities.

Steps:

1.	Open the INewsletterService.cs file in the Services folder.

	> Services/INewsletterService.cs
	
	```csharp
	using Newsletter.Models;
	
	namespace Newsletter.Services;
	
	public interface INewsletterService
	{
	    Task<ValidationResult> EnlistSubscriberAsync(Subscriber subscriber);
	    Task<IEnumerable<Subscriber>> GetSubscribersAsync();
	    Task<ValidationResult> CancelSubscriptionAsync(string subscriberId);
	}
	```

2. Update the NewsletterService

	Modify the NewsletterService to implement the new methods for fetching subscribers and handling unsubscriptions.
	
	Steps:
		1.	Open NewsletterService.cs in the Services folder.
	
	> Services/NewsletterService.cs
		
	```csharp
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
	    
	    public async Task<IEnumerable<Subscriber>> GetSubscribersAsync()
	    {
	        return await _subscriberRepository.GetSubscribersAsync();
	    }
	    
	    public async Task<ValidationResult> CancelSubscriptionAsync(string subscriberEmail)
	    {
	        var subscriber = await _subscriberRepository.GetSubscriberByEmailAsync(subscriberEmail);
	        if (subscriber == null || subscriber.Id == null)
	        {
	            return ValidationResult.Failure("Subscriber not found.");
	        }
	    
	        await _subscriberRepository.RemoveSubscriberAsync(subscriber.Id);
	        return ValidationResult.Success();
	    }
	}
	```
		
	> Purpose: The service now includes:
	> 
	> - GetSubscribersAsync: Fetches all subscribers from the repository.
	> - CancelSubscriptionAsync: Handles unsubscribing a subscriber by their ID with validation.

### 1. Add Code for the Subscriber List

We will protect the subscriber list page using the `[Authorize]` attribute to ensure only logged-in users can access it.

#### Steps:

1. Open `NewsletterController.cs` in the `Controllers` folder.
2. Add an action named `Subscribers` to retrieve the list of subscribers from the repository and pass it to the view.
3. Mark the `Subscribers` action with the `[Authorize]` attribute.
4. Add an `Unsubscribe` action to handle the removal of subscribers.

> Controllers/NewsletterController.cs

```csharp
...

    [Authorize]
    public async Task<IActionResult> Subscribers()
    {
        var subscribers = await _newsletterService.GetSubscribersAsync();
        return View(subscribers);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Unsubscribe(string email)
    {
        var result = await _newsletterService.CancelSubscriptionAsync(email);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Subscriber with email '{email}' has been unsubscribed.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Subscriber with email '{email}' was not found.";
        }

        return RedirectToAction("Subscribers");
    }

...

```

> **Purpose:**
> 
> - **`Subscribers` Action:** Fetches all subscribers and passes them to the view.
> - **`Unsubscribe` Action:** Removes a subscriber by their ID and sets a success message if the operation is successful.


### 2. Create the Subscribers View

The view will display the list of subscribers and include an unsubscribe button for each row.

#### Steps:

1. Navigate to the `Views/Newsletter` folder.
2. Create a new Razor view named `Subscribers.cshtml`.
3. Add the following code to display the list:

> Views/Newsletter/Subscribers.cshtml

```csharp
@model List<Newsletter.Models.Subscriber>

<!-- Display Message -->
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"><i class="fas fa-times"></i></button>
    </div>
}
@if (TempData["ErrorMessage"] != null)
{
    <div class="alert alert-danger alert-dismissible fade show" role="alert">
        @TempData["ErrorMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"><i class="fas fa-times"></i></button>
    </div>
}

<!-- List all subscribers -->
<h2 class="my-4">Newsletter Subscribers</h2>
<table class="table table-striped table-bordered">
    <thead class="thead-dark">
        <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var subscriber in Model)
        {
            <tr>
                <td>@subscriber.Name</td>
                <td>@subscriber.Email</td>
                <td>
                    <form method="post" asp-action="Unsubscribe" asp-controller="Newsletter">
                        <input type="hidden" name="email" value="@subscriber.Email" />
                        <button type="submit" class="btn btn-link text-danger btn-sm p-0" style="text-decoration: underline;">
                            Unsubscribe
                        </button>
                    </form>
                </td>
            </tr>
        }
    </tbody>
</table>
```

> **Purpose:**
> 
> - Displays the subscriber list in a table format.
> - Includes an unsubscribe button for each subscriber, which sends the `subscriberId` to the `Unsubscribe` action.


### 3. Add the Navigation Bar Link

We need to update the navigation bar to include a **Subscribers** link that only appears when the user is logged in.

#### Steps:

1. Open `_Layout.cshtml` in the `Views/Shared` folder.
2. Add the following code inside the navigation bar:

> Views/Shared/_Layout.cshtml

```html
@if (User.Identity != null && User.Identity.IsAuthenticated)
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Newsletter" asp-action="Subscribers">Subscribers</a>
    </li>
}
```

> **Purpose:**
> 
> - The **Subscribers** link is only visible when the user is authenticated.


### 4. Test the Implementation

#### Steps:

1. **Start the Application:**

   ```bash
   dotnet run
   ```

2. **Test Access Control:**
   - Try accessing the `/Newsletter/Subscribers` page while logged out (should redirect to login).
   - Log in and verify you can access the page.

3. **Test Unsubscribe Functionality:**
   - Unsubscribe a user from the list and verify the success message is displayed.


## Summary

In this tutorial, we:

1. Used the `[Authorize]` attribute to protect the subscriber list page.
2. Created the `Subscribers` view to display all subscribers with an unsubscribe button for each user.
3. Updated the navigation bar to dynamically show or hide the **Subscribers** link based on login status.
4. Ensured the unsubscribe functionality works as expected and displays success messages.

# Subscribe 🎉

