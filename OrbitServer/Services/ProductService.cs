using MongoDB.Driver;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;

    public ProductService(MongoDbContext dbContext)
    {
        _products = dbContext.GetCollection<Product>("Products");
    }

    public async Task<List<Product>> GetProductsAsync() =>
        await _products.Find(p => true).ToListAsync();

    public async Task<Product> GetProductByIdAsync(string id) =>
        await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task CreateProductAsync(Product product) =>
        await _products.InsertOneAsync(product);

    public async Task UpdateProductAsync(string id, Product product) =>
        await _products.ReplaceOneAsync(p => p.Id == id, product);

    public async Task DeleteProductAsync(string id) =>
        await _products.DeleteOneAsync(p => p.Id == id);
}
