using MongoDB.Driver;

public class OrderService
{
    private readonly IMongoCollection<Order> _orders;
    private readonly IMongoCollection<OrderItem> _orderItems;
    private readonly IMongoCollection<Cart> _carts;

    public OrderService(MongoDbContext dbContext)
    {
        _orders = dbContext.GetCollection<Order>("Orders");
        _orderItems = dbContext.GetCollection<OrderItem>("OrderItems");
        _carts = dbContext.GetCollection<Cart>("Carts");
    }

    public async Task<List<Cart>> GetCartsAsync() =>
        await _carts.Find(c => true).ToListAsync();

    public async Task<Cart> GetCartByIdAsync(string id) =>
        await _carts.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task CreateCartAsync(Cart cart) =>
        await _carts.InsertOneAsync(cart);

    public async Task UpdateCartAsync(string id, Cart cart) =>
        await _carts.ReplaceOneAsync(c => c.Id == id, cart);

    public async Task DeleteCartAsync(string id) =>
        await _carts.DeleteOneAsync(c => c.Id == id);

    public async Task<Cart> GetCartByCustomerIdAsync(string id) =>
        await _carts.Find(c => c.Customer.Id == id).FirstOrDefaultAsync();
}
