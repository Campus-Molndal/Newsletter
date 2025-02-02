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
