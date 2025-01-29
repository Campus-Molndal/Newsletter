using Newsletter.Models;

namespace Newsletter.Data;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    private readonly List<Subscriber> _subscribers = new();

    public Task AddSubscriberAsync(Subscriber subscriber)
    {
        subscriber.Id = Guid.NewGuid().ToString();
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
