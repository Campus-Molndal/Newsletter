+++
title = "6. Use the Repository Pattern (In-Memory DB)"
weight = 6
date = 2025-01-28
draft = false
+++

## Goal

In this tutorial, we will add support for an in-memory database to store subscribers using the **repository pattern**. The repository will act as an abstraction layer to manage data storage and retrieval. We will also update the service layer to utilize the repository, replacing the current mocked list.

## Step-by-step Guide

### 1. Define the `ISubscriberRepository` Interface

The `ISubscriberRepository` interface defines the contract for interacting with subscriber data. This includes methods for adding, retrieving, and managing subscribers.

#### Steps:

1. Create a `Data` folder in the project if it doesn’t already exist.
2. Add a new file named `ISubscriberRepository.cs` in the `Data ` folder.

> Data/ISubscriberRepository.cs

```csharp
using Newsletter.Models;

namespace Newsletter.Data;

public interface ISubscriberRepository
{
    Task AddSubscriberAsync(Subscriber subscriber);
    Task<IEnumerable<Subscriber>> GetSubscribersAsync();
    Task<Subscriber?> GetSubscriberByIdAsync(string id);
    Task<Subscriber?> GetSubscriberByEmailAsync(string email);
    Task RemoveSubscriberAsync(string id);
}
```

> **Purpose:** The interface defines methods for common operations like adding, retrieving, and deleting subscribers, enabling a consistent and testable approach to data access. CRUD operations.

### 2. Implement the `InMemorySubscriberRepository`

The `InMemorySubscriberRepository` provides an in-memory implementation of the repository for managing subscriber data.

#### Steps:

1. Add a new file named `InMemorySubscriberRepository.cs` in the `Data` folder.

> Data/InMemorySubscriberRepository.cs

```csharp
using Newsletter.Models;

namespace Newsletter.Data;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    private readonly List<Subscriber> _subscribers = new();

    public Task AddSubscriberAsync(Subscriber subscriber)
    {
        _subscribers.Add(subscriber);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Subscriber>> GetSubscribersAsync()
    {
        return Task.FromResult<IEnumerable<Subscriber>>(_subscribers);
    }

    public Task<Subscriber?> GetSubscriberByIdAsync(string id)
    {
        return Task.FromResult(_subscribers.FirstOrDefault(s => s.Id == id));
    }

    public Task<Subscriber?> GetSubscriberByEmailAsync(string email)
    {
        return Task.FromResult(_subscribers.FirstOrDefault(s => s.Email == email));
    }

    public Task RemoveSubscriberAsync(string id)
    {
        var subscriber = _subscribers.FirstOrDefault(s => s.Id == id);
        if (subscriber != null)
        {
            _subscribers.Remove(subscriber);
        }
        return Task.CompletedTask;
    }
}
```

> **Purpose:** This implementation stores subscriber data in memory and provides CRUD operations to manage it. It simulates database behavior for educational purposes.


### 3. Register the Repository in Dependency Injection

To use the repository in the service layer, it must be registered in the dependency injection container.

#### Steps:

1. Open `Program.cs`. Add the repository registration to the service collection:

> Program.cs

```csharp
using Newsletter.Data;

...

// Register the subscriber repository in the DI container
builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();

...
```

> **Purpose:** Dependency injection ensures that the repository can be injected wherever needed, promoting loose coupling and testability.


### 4. Update the `NewsletterService`

Modify the `NewsletterService` to use the repository instead of the mocked in-memory list.

#### Steps:

1. Open `NewsletterService.cs` in the `Services` folder.

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
}
```

> **Purpose:** The service now delegates data management tasks to the repository while focusing on implementing use-case-specific logic.


### 5. Test the Changes

#### Steps:

1. Run the application:

   ```bash
   dotnet run
   ```

2. Test the following scenarios:
   - Subscribing a new user.
   - Subscribing with a duplicate email (should show an error).
   - Unsubscribing a user by ID.
   - Verifying the subscriber list using the in-memory database.

---

## Lessons Learned

In this tutorial, we:

1. Defined the `ISubscriberRepository` interface to abstract data access.
2. Implemented the `InMemorySubscriberRepository` to simulate a database.
3. Registered the repository in the dependency injection container.
4. Updated the service to delegate data management to the repository.
5. Verified that the repository pattern provides a flexible and testable approach to managing data.

You have now used the repository pattern! 🎉

