using MongoDB.Driver;

public class ProductService
{
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Category> _categories;

    public ProductService(MongoDbContext dbContext)
    {
        _products = dbContext.GetCollection<Product>("Products");
        _categories = dbContext.GetCollection<Category>("Categories");
    }

    public async Task<List<Product>> GetProductsAsync() =>
        await _products.Find(p => true).ToListAsync();

    public async Task<Product> GetProductByIdAsync(string id) =>
        await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task<List<Product>> GetProductsByVendorIdAsync(string id) =>
        await _products.Find(p => p.Vendor.Id == id).ToListAsync();

    public async Task CreateProductAsync(Product product) =>
        await _products.InsertOneAsync(product);

    public async Task UpdateProductAsync(string id, Product product) =>
        await _products.ReplaceOneAsync(p => p.Id == id, product);

    public async Task DeleteProductAsync(string id) =>
        await _products.DeleteOneAsync(p => p.Id == id);

    public async Task CreateCategoryAsync(Category category) =>
        await _categories.InsertOneAsync(category);

    public async Task UpdateCategoryAsync(string id, Category category) =>
        await _categories.ReplaceOneAsync(c => c.Id == id, category);

    public async Task<Category> GetCategoryByIdAsync(string id) =>
        await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<List<Category>> GetCategoriesAsync() =>
        await _categories.Find(c => true).ToListAsync();

    public async Task<List<Category>> GetActivatedCategoriesAsync() =>
        await _categories.Find(c => c.Status == true).ToListAsync();
}
