using MongoDB.Driver;

public class RatingService
{
    private readonly IMongoCollection<VendorRating> _ratings;

    public RatingService(MongoDbContext dbContext)
    {
        _ratings = dbContext.GetCollection<VendorRating>("VendorRatings");
    }

    public async Task<List<VendorRating>> GetVendorRatingsAsync() =>
        await _ratings.Find(r => true).ToListAsync();

    public async Task<VendorRating> GetVendorRatingByIdAsync(string id) =>
        await _ratings.Find(r => r.Id == id).FirstOrDefaultAsync();

    public async Task CreateVendorRatingAsync(VendorRating vendorRating) =>
        await _ratings.InsertOneAsync(vendorRating);

    public async Task UpdateVendorRatingAsync(string id, VendorRating vendorRating) =>
        await _ratings.ReplaceOneAsync(r => r.Id == id, vendorRating);

    public async Task DeleteVendorRatingAsync(string id) =>
        await _ratings.DeleteOneAsync(r => r.Id == id);

    public async Task<List<VendorRating>> GetVendorRatingsByVendorIdAsync(string id) =>
        await _ratings.Find(r => r.Vendor.Id == id).ToListAsync();

    public async Task<List<VendorRating>> GetVendorRatingsByCustomerIdAsync(string id) =>
        await _ratings.Find(r => r.Customer.Id == id).ToListAsync();

    public async Task<VendorRating> GetVendorRatingByCustomerAndVendorIdAsync(string customerId, string vendorId) =>
        await _ratings.Find(r => r.Customer.Id == customerId && r.Vendor.Id == vendorId).FirstOrDefaultAsync();
}
