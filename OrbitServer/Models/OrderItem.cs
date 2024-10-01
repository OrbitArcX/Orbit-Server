using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class OrderItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public Product Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public User Vendor { get; set; }
    public User Customer { get; set; }
    public string? OrderId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
