using Newsletter.Models;

namespace Newsletter.Data;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    private readonly List<Subscriber> _subscribers = new();
    private readonly object _lock = new();

    public Task AddSubscriberAsync(Subscriber subscriber)
    {
        lock (_lock)
        {
            subscriber.Id = Guid.NewGuid().ToString();
            _subscribers.Add(subscriber);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Subscriber>> GetSubscribersAsync()
    {
        IEnumerable<Subscriber> subscribersSnapshot;
        lock (_lock)
        {
            subscribersSnapshot = _subscribers.ToList();
        }
        return Task.FromResult(subscribersSnapshot);
    }

    public Task<Subscriber?> GetSubscriberByIdAsync(string id)
    {
        Subscriber? subscriber;
        lock (_lock)
        {
            subscriber = _subscribers.FirstOrDefault(s => s.Id == id);
        }
        return Task.FromResult(subscriber);
    }

    public Task<Subscriber?> GetSubscriberByEmailAsync(string email)
    {
        Subscriber? subscriber;
        lock (_lock)
        {
            subscriber = _subscribers.FirstOrDefault(s => s.Email == email);
        }
        return Task.FromResult(subscriber);
    }

    public Task RemoveSubscriberAsync(string id)
    {
        lock (_lock)
        {
            var subscriber = _subscribers.FirstOrDefault(s => s.Id == id);
            if (subscriber != null)
            {
                _subscribers.Remove(subscriber);
            }
        }
        return Task.CompletedTask;
    }
}
