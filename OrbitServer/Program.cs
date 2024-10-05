using MongoDB.Driver;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the enum string serialization configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Cloudinary settings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// MongoDB settings and context
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDbConnection")));
builder.Services.AddScoped<MongoDbContext>();  // MongoDB context

// Add services for business logic
builder.Services.AddScoped<ProductService>();  // Add ProductService
builder.Services.AddScoped<CloudinaryService>();  // Add CloudinaryService
builder.Services.AddScoped<UserService>();  // Add UserService
builder.Services.AddScoped<OrderService>();  // Add OrderService
builder.Services.AddScoped<NotificationService>();  // Add NotificationService
builder.Services.AddScoped<RatingService>();  // Add RatingService

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", builder =>
    {
        builder.AllowAnyOrigin()  // Allow any origin
               .AllowAnyHeader()  // Allow any header
               .AllowAnyMethod();  // Allow any method (GET, POST, etc.)
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Enable routing and CORS
app.UseRouting();
app.UseCors("AllowFrontendApp");  // Apply the CORS policy

app.MapControllers();

// Start the app
app.Run();

// WeatherForecast record
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}