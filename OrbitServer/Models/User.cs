// File Name: User.cs
// Description: User model

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Email { get; set; }
    public string Name { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; }  // Admin, Vendor, CSR, Customer
    public bool Status { get; set; }
    public bool ApproveStatus { get; set; }
    public decimal Rating { get; set; }
    public decimal RatingCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
