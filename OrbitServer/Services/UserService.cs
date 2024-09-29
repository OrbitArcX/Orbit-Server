using MongoDB.Driver;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(MongoDbContext dbContext)
    {
        _users = dbContext.GetCollection<User>("Users");
    }

    public async Task<List<User>> GetUsersAsync() =>
        await _users.Find(p => true).ToListAsync();

    public async Task<User> GetUserByIdAsync(string id) =>
        await _users.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateUserAsync(User user) =>
        await _users.InsertOneAsync(user);

    public async Task UpdateUserAsync(string id, User user) =>
        await _users.ReplaceOneAsync(p => p.Id == id, user);

    public async Task DeleteUserAsync(string id) =>
        await _users.DeleteOneAsync(p => p.Id == id);

    public async Task<User> UserLogin(LoginDto loginDto) =>
        await _users.Find(p => p.Email == loginDto.Username).FirstOrDefaultAsync();
}
