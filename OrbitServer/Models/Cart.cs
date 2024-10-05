// File Name: Cart.cs
// Description: Shopping cart model

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Cart
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public List<CartItem>? CartItems { get; set; }
    public User Customer { get; set; }
    public decimal CartPrice { get; set; }
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
