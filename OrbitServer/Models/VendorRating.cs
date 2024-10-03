// File Name: VendorRating.cs
// Description: Vendor rating model

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class VendorRating
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Comment { get; set; }
    public decimal Rating { get; set; }
    public User Vendor { get; set; }
    public User Customer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
