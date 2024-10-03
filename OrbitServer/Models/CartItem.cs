// File Name: CartItem.cs
// Description: Shopping cart item model

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CartItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
