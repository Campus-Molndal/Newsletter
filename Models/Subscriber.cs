using System.ComponentModel.DataAnnotations;

namespace Newsletter.Models;

public class Subscriber
{
    public string? Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required, EmailAddress]
    public string? Email { get; set; }
}