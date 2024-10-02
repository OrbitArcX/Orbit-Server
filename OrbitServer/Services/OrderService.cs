using CloudinaryDotNet.Core;
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

    public async Task<List<Order>> GetOrdersAsync() =>
        await _orders.Find(o => true).ToListAsync();

    public async Task<Order> GetOrderByIdAsync(string id) =>
        await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();

    public async Task CreateOrderAsync(Order order) =>
        await _orders.InsertOneAsync(order);

    public async Task UpdateOrderAsync(string id, Order order) =>
        await _orders.ReplaceOneAsync(o => o.Id == id, order);

    public async Task DeleteOrderAsync(string id) =>
        await _orders.DeleteOneAsync(o => o.Id == id);

    public async Task<List<Order>> GetOrdersByCustomerIdAsync(string id) =>
        await _orders.Find(o => o.Customer.Id == id).ToListAsync();

    public async Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status) =>
        await _orders.Find(o => o.Status == status).ToListAsync();

    public async Task<List<Order>> GetCancelRequestOrdersAsync() =>
        await _orders.Find(o => o.CancelRequest == true).ToListAsync();

    public async Task<List<OrderItem>> GetOrderItemsAsync() =>
        await _orderItems.Find(oi => true).ToListAsync();

    public async Task<OrderItem> GetOrderItemByIdAsync(string id) =>
        await _orderItems.Find(oi => oi.Id == id).FirstOrDefaultAsync();

    public async Task CreateOrderItemAsync(OrderItem orderItem) =>
        await _orderItems.InsertOneAsync(orderItem);

    public async Task UpdateOrderItemAsync(string id, OrderItem orderItem) =>
        await _orderItems.ReplaceOneAsync(oi => oi.Id == id, orderItem);

    public async Task DeleteOrderItemAsync(string id) =>
        await _orderItems.DeleteOneAsync(oi => oi.Id == id);

    public async Task<List<OrderItem>> GetOrderItemsByCustomerIdAsync(string id) =>
        await _orderItems.Find(oi => oi.Customer.Id == id).ToListAsync();

    public async Task<List<OrderItem>> GetOrderItemsByVendorIdAsync(string id) =>
        await _orderItems.Find(oi => oi.Vendor.Id == id).ToListAsync();

    public async Task<List<OrderItem>> GetOrderItemsByStatusAsync(OrderStatus status) =>
        await _orderItems.Find(oi => oi.Status == status).ToListAsync();

    public async Task<List<OrderItem>> GetOrderItemsByProductIdAsync(string id) =>
        await _orderItems.Find(oi => oi.Product.Id == id).ToListAsync();
}
