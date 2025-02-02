+++
title = "9. Add Repository for MongoDB"
weight = 9
date = 2025-02-02
draft = false
+++

## Goal

In this tutorial, you will learn how to:

1.	Add the MongoDB driver package to your ASP.NET project.
2.	Configure MongoDB connection settings in appsettings.json.
3.	Create a MongoDbSettings class to manage MongoDB configuration.
4.	Implement the MongoDbSubscriberRepository to handle CRUD operations.
5.	Register the MongoDB repository in Program.cs for dependency injection.

## Step-by-step Guide

### 1. Add the MongoDB Driver Package

We need to add the official MongoDB driver to our project to interact with MongoDB.

Steps:

1.	Open your terminal and navigate to your project directory.
2.	Run the following command to add the MongoDB driver package:

```bash
dotnet add package MongoDB.Driver  
```

> Purpose:
> 
> This step installs the necessary package to enable MongoDB support in the project.

### 2. Configure MongoDB in appsettings.json

We’ll add MongoDB connection details to the configuration file for easier management.

Steps:

1.	Open `appsettings.json` in the root of your project. Add the following section for MongoDB configuration:

> appsettings.json

```jason
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "SECRET - Set your connection string here",
    "DatabaseName": "Set your newsletter database name here",
    "CollectionName": "Set your newsletter collection name here"
  },
  "DatabaseToUse": "Choose your database here - MongoDb or InMemoryDb"
}
```

> Purpose:
> 
> This is the default settings, so we will use this file to document all possible configurations. Later we will override this file for every environment we aim to deploy in. 


2. Open `appsettings.Development.json` in the root of your project. Add the following section for MongoDB configuration:

> appsettings.Development.json

```jason
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NewsletterDb",
    "CollectionName": "Subscribers"
  },
  "DatabaseToUse": "MongoDb"
}
```

> Purpose:
> 
> - This stores the MongoDB connection string and database name in a centralized configuration file, making it easier to update without modifying the code.
> - This file will be used in the Development environment. In our case it is the local environment on our laptop.


### 3. Create the MongoDbSettings Class

We’ll create a class to map the MongoDB settings from appsettings.json for use in the application. We will use the _Options Pattern_, which helps us handle configurations in a strongly typed manor.

Steps:

1.	Create a new `Configurations` folder and create a new file named `MongoDbSettings.cs`.
2.	Add the following code to map the configuration:

> Configurations/MongoDbSettings.cs

```csharp
namespace Newsletter.Configurations;

public class MongoDBSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
}
```

> Purpose:
> 
> This class represents the MongoDB settings, allowing us to inject and access them easily in the application.

### 4. Implement the MongoDbSubscriberRepository

We’ll create a repository that handles adding, retrieving, and querying subscribers from MongoDB - CRUD operations.

Steps:

1.	Navigate to the `Data` folder.
2.	Create a new file named `MongoDbSubscriberRepository.cs`.
3.	Add the following code to implement MongoDB operations:

> Data/MongoDbSubscriberRepository.cs

```csharp
using MongoDB.Driver;
using Newsletter.Models;

namespace Newsletter.Data;

public class MongoDbSubscriberRepository : ISubscriberRepository
{

        private readonly IMongoCollection<Subscriber> _subscribers;

    public MongoDbSubscriberRepository(IMongoCollection<Subscriber> subscriberCollection)
    {
        _subscribers = subscriberCollection;
    }

    public async Task AddSubscriberAsync(Subscriber subscriber)
    {
        await _subscribers.InsertOneAsync(subscriber);
    }

    public async Task<IEnumerable<Subscriber>> GetSubscribersAsync()
    {
        return await _subscribers.Find(_ => true).ToListAsync();
    }

    public async Task<Subscriber?> GetSubscriberByIdAsync(string id)
    {
        return await _subscribers.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Subscriber?> GetSubscriberByEmailAsync(string email)
    {
        return await _subscribers.Find(s => s.Email == email).FirstOrDefaultAsync();
    }

    public async Task RemoveSubscriberAsync(string id)
    {
        await _subscribers.DeleteOneAsync(s => s.Id == id);
    }
}
```

> Purpose:
> 
> - This repository provides methods to add and retrieve subscribers from the MongoDB database using the settings from appsettings.json. (CRUD operation)
> - Note that we give the class a reference to a MongoDB Collection and not the entire database. This is a design choice in this case. We have to remeber this when we register the repository in the DI container in order to configure it correctly. In order to keep the repository class agnostic to configurations, we do all configurations in Program.cs instead.


### 5. Update the Subscriber Model for MongoDB ID Management

MongoDB uses ObjectId as the default type for document IDs. To ensure proper storage and retrieval of subscribers, we need to adjust the Subscriber model to align with MongoDB’s ID handling conventions.

Steps:

1.	Open the `Subscriber.cs` file located in the `Models` folder.
2.	Update the Id property to use the correct MongoDB annotations, ensuring the ID is stored as an ObjectId in the database but handled as a string in the application.

> Models/Subscriber.cs

```csharp
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Newsletter.Models;

public class Subscriber
{
    [BsonId] // Marks this property as the document's primary key
    [BsonRepresentation(BsonType.ObjectId)] // Stores the Id as an ObjectId in MongoDB
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
}
```

> Explanation:
> 
> - [BsonId]: This attribute marks the Id property as the primary key for MongoDB documents.
> - [BsonRepresentation(BsonType.ObjectId)]: This ensures that the Id is stored as an ObjectId in MongoDB but treated as a string in the application. This approach simplifies interactions in C# without losing compatibility with MongoDB’s ID format.
> 
> Purpose:
> 
> This step ensures that your Subscriber model properly interacts with MongoDB’s default ID handling, allowing for smooth CRUD operations.
> 
> Note: 
> 
> If you’re migrating from an in-memory repository, remember that MongoDB automatically generates the Id when inserting documents. There’s no need to manually assign Id values unless explicitly required.


### 6. Register MongoDB Services in Program.cs

We need to register the MongoDB settings and repository in the dependency injection container so they can be used throughout the application. We also want to be able to alternate between different repository solutions (like our previous In-memory database). This makes it easy to test without being dependent on the MongoDB.

Steps:

1.	Open Program.cs in the project root.
2.	Add the following code to register MongoDB services:

> Program.cs

```csharp

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newsletter.Configurations;
using Newsletter.Data;
using Newsletter.Models;
using Newsletter.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add support for basic authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options => options.LoginPath = "/Account/Login");

// Register the newsletter service in the DI container
builder.Services.AddSingleton<INewsletterService, NewsletterService>();

// Insert the correct database repository based on the configuration
var databaseToUse = builder.Configuration["DatabaseToUse"];

switch (databaseToUse)
{
    // Add the InMemoryDb repository if the configuration is set to InMemoryDb or if the configuration is not set
    case "InMemoryDb":
    case var db when string.IsNullOrEmpty(db):
        builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();
        break;
    case "MongoDb":
        builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDbSettings"));
        builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>(config =>
        {
            var settings = config.GetRequiredService<IOptions<MongoDBSettings>>().Value;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            var collection = database.GetCollection<Subscriber>(settings.CollectionName);
            return new MongoDbSubscriberRepository(collection);
        });
        break;
    default:
        throw new InvalidOperationException("Invalid database configuration");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
```

> Purpose:
> 
> - builder.Services.Configure<MongoDbSettings>: Binds the MongoDB settings from appsettings.json.
> - builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>: Registers the MongoDB repository for dependency injection.

### 7. Run and Test the Application

Let’s verify that the MongoDB integration works correctly.

Steps:

1.	Start the MongoDB container (if not already running):

	```bash
	docker compose -f ./infra/mongodb_and_express.yaml up -d  
	```

2.	Run the ASP.NET application:
	
	```bash
	dotnet run  
	```

3.	Test the subscription flow:
	- Go to http://localhost:<port>/Newsletter/Subscribe.
	- Submit a subscription form with a name and email.
4.	Verify in Mongo Express:
	- Navigate to http://localhost:8081 to see if the subscriber data appears in the Subscribers collection.


## Summary

In this tutorial, we:

1.	Installed the MongoDB driver package using dotnet add package MongoDB.Driver.
2.	Configured MongoDB connection details in appsettings.json.
3.	Created a MongoDbSettings class to manage configuration.
4.	Implemented the MongoDbSubscriberRepository to handle subscriber data.
5.	Registered the MongoDB repository and settings in Program.cs.
6.	Successfully ran the application and verified data integration with MongoDB.

# Now your application is connected to MongoDB! 🚀