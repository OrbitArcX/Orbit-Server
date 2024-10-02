using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; }
    public string Body { get; set; }
    public User User { get; set; }
    public bool SeenStatus { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
