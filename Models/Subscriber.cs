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