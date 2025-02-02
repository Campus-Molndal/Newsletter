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
