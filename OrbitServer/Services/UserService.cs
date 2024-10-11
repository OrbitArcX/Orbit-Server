// File Name: UserService.cs
// Description: User service to define repository methods to handle user data in the database

using MongoDB.Driver;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbContext dbContext)
    {
        _users = dbContext.GetCollection<User>("Users");
    }

    public async Task<List<User>> GetUsersAsync() =>
        await _users.Find(u => true).ToListAsync();

    public async Task<User> GetUserByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User> GetUserByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task CreateUserAsync(User user) =>
        await _users.InsertOneAsync(user);

    public async Task UpdateUserAsync(string id, User user) =>
        await _users.ReplaceOneAsync(u => u.Id == id, user);

    public async Task DeleteUserAsync(string id) =>
        await _users.DeleteOneAsync(u => u.Id == id);

    public async Task<User> UserLogin(LoginDto loginDto) =>
        await _users.Find(u => u.Email == loginDto.Username).FirstOrDefaultAsync();

    public async Task<List<User>> GetUsersToApproveLoginAsync() =>
        await _users.Find(u => u.ApproveStatus == false).ToListAsync();

    public async Task<List<User>> GetUsersByRoleAsync(string role) =>
        await _users.Find(u => u.Role == role).ToListAsync();
}
