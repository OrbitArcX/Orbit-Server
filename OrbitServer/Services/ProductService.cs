// File Name: ProductService.cs
// Description: Product service to define repository methods to handle product data in the database

using MongoDB.Driver;
using MongoDB.Bson;

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

    public async Task<List<Product>> GetProductsByCategoryIdAsync(string id) =>
        await _products.Find(p => p.Category.Id == id).ToListAsync();

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

    // Product search with sorting by price and vendor rating
    public async Task<List<Product>> SearchProductsAsync(
        string? name,
        string? author,
        decimal? minPrice,
        decimal? maxPrice,
        string? vendorId,
        string? categoryId,
        decimal? minRating,
        decimal? maxRating,
        string? sortBy = null,
        bool isAscending = true)
    {
        var filter = Builders<Product>.Filter.Empty;

        // Filter by name (case insensitive)
        if (!string.IsNullOrEmpty(name))
        {
            var nameFilter = Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(name, "i"));
            var authorFilter = Builders<Product>.Filter.Regex(p => p.Author, new BsonRegularExpression(name, "i"));
            var categoryNameFilter = Builders<Product>.Filter.Regex(p => p.Category.Name, new BsonRegularExpression(name, "i"));

            filter &= Builders<Product>.Filter.Or(nameFilter, authorFilter, categoryNameFilter);
        }

        // Filter by author (case insensitive)
        if (!string.IsNullOrEmpty(author))
        {
            filter &= Builders<Product>.Filter.Regex(p => p.Author, new BsonRegularExpression(author, "i"));
        }

        // Filter by price range
        if (minPrice.HasValue && maxPrice.HasValue)
        {
            filter &= Builders<Product>.Filter.Gte(p => p.Price, minPrice.Value) &
                      Builders<Product>.Filter.Lte(p => p.Price, maxPrice.Value);
        }
        else if (minPrice.HasValue)
        {
            filter &= Builders<Product>.Filter.Gte(p => p.Price, minPrice.Value);
        }
        else if (maxPrice.HasValue)
        {
            filter &= Builders<Product>.Filter.Lte(p => p.Price, maxPrice.Value);
        }

        // Filter by vendor
        if (!string.IsNullOrEmpty(vendorId))
        {
            filter &= Builders<Product>.Filter.Eq(p => p.Vendor.Id, vendorId);
        }

        // Filter by category
        if (!string.IsNullOrEmpty(categoryId))
        {
            filter &= Builders<Product>.Filter.Eq(p => p.Category.Id, categoryId);
        }

        // Filter by rating range (using vendor rating)
        if (minRating.HasValue && maxRating.HasValue)
        {
            filter &= Builders<Product>.Filter.Gte(p => p.Vendor.Rating, minRating.Value) &
                      Builders<Product>.Filter.Lte(p => p.Vendor.Rating, maxRating.Value);
        }
        else if (minRating.HasValue)
        {
            filter &= Builders<Product>.Filter.Gte(p => p.Vendor.Rating, minRating.Value);
        }
        else if (maxRating.HasValue)
        {
            filter &= Builders<Product>.Filter.Lte(p => p.Vendor.Rating, maxRating.Value);
        }

        var sort = Builders<Product>.Sort.Ascending(p => p.Name); // Default sort

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("price", StringComparison.OrdinalIgnoreCase))
            {
                sort = isAscending ? Builders<Product>.Sort.Ascending(p => p.Price)
                                   : Builders<Product>.Sort.Descending(p => p.Price);
            }
            else if (sortBy.Equals("rating", StringComparison.OrdinalIgnoreCase))
            {
                sort = isAscending ? Builders<Product>.Sort.Ascending(p => p.Vendor.Rating)
                                   : Builders<Product>.Sort.Descending(p => p.Vendor.Rating);
            }
        }

        return await _products.Find(filter).Sort(sort).ToListAsync();
    }
}
