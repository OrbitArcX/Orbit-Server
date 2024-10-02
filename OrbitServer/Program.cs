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

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000")  // Allow the frontend origin
               .AllowAnyHeader()                     // Allow any header
               .AllowAnyMethod()                     // Allow any method (GET, POST, etc.)
               .AllowCredentials();                  // Allow credentials if needed
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable routing and CORS
app.UseRouting();
app.UseCors("AllowFrontendApp");  // Apply the CORS policy

app.MapControllers();

// Weather Forecast Endpoint
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Start the app
app.Run();

// WeatherForecast record
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}