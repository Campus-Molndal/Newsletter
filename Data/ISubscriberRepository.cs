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
