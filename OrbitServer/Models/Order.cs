// File Name: Order.cs
// Description: Order model

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public List<OrderItem> OrderItems { get; set; }
    public User Customer { get; set; }
    public decimal OrderPrice { get; set; }
    public bool CancelRequest { get; set; }
    public string? CancelReason { get; set; }

    [BsonRepresentation(BsonType.String)]
    public OrderStatus? Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
