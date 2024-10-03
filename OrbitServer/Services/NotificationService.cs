// File Name: NotificationService.cs
// Description: Notification service to define repository methods to handle notification data in the database

using MongoDB.Driver;

public class NotificationService
{
    private readonly IMongoCollection<Notification> _notifications;

    public NotificationService(MongoDbContext dbContext)
    {
        _notifications = dbContext.GetCollection<Notification>("Notifications");
    }

    public async Task<List<Notification>> GetNotificationsByUserIdAsync(string id) =>
        await _notifications.Find(n => n.User.Id == id).ToListAsync();

    public async Task<List<Notification>> GetUnseenNotificationsByUserIdAsync(string id) =>
    await _notifications.Find(n => n.User.Id == id && n.SeenStatus == false).ToListAsync();

    public async Task<Notification> GetNotificationByIdAsync(string id) =>
        await _notifications.Find(n => n.Id == id).FirstOrDefaultAsync();

    public async Task CreateNotificationAsync(Notification notification) =>
        await _notifications.InsertOneAsync(notification);

    public async Task UpdateNotificationAsync(string id, Notification notification) =>
        await _notifications.ReplaceOneAsync(n => n.Id == id, notification);

    public async Task DeleteNotificationAsync(string id) =>
        await _notifications.DeleteOneAsync(n => n.Id == id);
}
