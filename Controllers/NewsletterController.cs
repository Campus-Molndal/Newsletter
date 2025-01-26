using Microsoft.AspNetCore.Mvc;
using Newsletter.Models;

namespace Newsletter.Controllers;

public class NewsletterController : Controller
{
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
            await Task.Delay(100); // Simulate an operation

            // Set a result message in TempData in order to display it in the view
            TempData["SuccessMessage"] = "You have successfully subscribed to our newsletter!";

            // Redirect back to the Subscribe GET action to clear the form
            return RedirectToAction("Subscribe");
        }

        // If the model state is invalid, redisplay the form with validation errors
        return View(subscriber);
    }

}