namespace Newsletter.Models;

public class Subscriber
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public required string Email { get; set; }
}