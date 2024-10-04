// File Name: ProductController.cs
// Description: Handles all product related business logic

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly CloudinaryService _cloudinaryService;
    private readonly UserService _userService;
    private readonly OrderService _orderService;

    public ProductsController(ProductService productService, CloudinaryService cloudinaryService, UserService userService, OrderService orderService)
    {
        _productService = productService;
        _cloudinaryService = cloudinaryService;
        _userService = userService;
        _orderService = orderService;
    }

    // Get all products
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetProductsAsync();
        return Ok(products);
    }

    // Get product by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    // Get all products by vendor id
    [HttpGet("vendor/{id}")]
    public async Task<IActionResult> GetProductsByVendorId(string id)
    {
        var vendor = await _userService.GetUserByIdAsync(id);

        if (vendor == null)
        {
            return BadRequest("Incorrect vendor id");
        }

        var products = await _productService.GetProductsByVendorIdAsync(id);
        return Ok(products);
    }

    // Get all products by category id
    [HttpGet("getby/category/{id}")]
    public async Task<IActionResult> GetProductsByCategoryId(string id)
    {
        var category = await _productService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return BadRequest("Incorrect category id");
        }

        var products = await _productService.GetProductsByCategoryIdAsync(id);
        return Ok(products);
    }

    // Create a new product with image upload
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDto createProductDto)
    {
        if (createProductDto == null || createProductDto.ImageFile == null)
        {
            return BadRequest("Product data or image file is missing");
        }

        var category = await _productService.GetCategoryByIdAsync(createProductDto.CategoryId);

        if (category == null || category.Status == false)
        {
            return BadRequest("Unavailable product category");
        }

        if (createProductDto.VendorId == null)
        {
            return BadRequest("Vendor details are missing");
        }

        var vendor = await _userService.GetUserByIdAsync(createProductDto.VendorId);

        if (vendor == null || !vendor.Role.Equals("Vendor", StringComparison.CurrentCultureIgnoreCase))
        {
            return BadRequest("Incorrect Vendor details");
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
            Category = category,
            Vendor = vendor,
        };

        if (createProductDto.ImageFile != null)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(createProductDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image upload failed");
            }

            product.ImageUrl = imageUrl;
        }

        if (createProductDto.Author != null)
        {
            product.Author = createProductDto.Author;
        }

        await _productService.CreateProductAsync(product);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // Update an existing product with an optional image update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromForm] ProductDto productDto)
    {
        var existingProduct = await _productService.GetProductByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        var category = await _productService.GetCategoryByIdAsync(productDto.CategoryId);

        if (category == null || category.Status == false)
        {
            return BadRequest("Unavailable product category");
        }

        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;
        existingProduct.Stock = productDto.Stock;
        existingProduct.Category = category;

        if (productDto.ImageFile != null)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(productDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image upload failed");
            }

            existingProduct.ImageUrl = imageUrl;
        }

        if (productDto.Author != null)
        {
            existingProduct.Author = productDto.Author;
        }

        existingProduct.UpdatedAt = DateTime.Now;
        await _productService.UpdateProductAsync(id, existingProduct);

        return Ok(existingProduct);
    }

    // Delete product by id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var existingProduct = await _productService.GetProductByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        var orderItems = await _orderService.GetOrderItemsByProductIdAsync(id);

        foreach (OrderItem orderItem in orderItems)
        {
            if (orderItem.Status == OrderStatus.Pending)
            {
                return BadRequest("Cannot delete products in a pending order");
            }
        }

        await _productService.DeleteProductAsync(id);
        return Ok($"Successfully deleted product with id: {id}");
    }

    // Get all product categories
    [HttpGet("category")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetCategoriesAsync();
        return Ok(categories);
    }

    // Get all activated product categories
    [HttpGet("category/activated")]
    public async Task<IActionResult> GetActivatedCategories()
    {
        var categories = await _productService.GetActivatedCategoriesAsync();
        return Ok(categories);
    }

    // Get product category by id
    [HttpGet("category/{id}")]
    public async Task<IActionResult> GetCategory(string id)
    {
        var category = await _productService.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    // Create product category
    [HttpPost("category")]
    public async Task<IActionResult> CreateCategory([FromForm] CategoryDto categoryDto)
    {
        if (categoryDto == null)
        {
            return BadRequest("Category data is missing");
        }

        var category = new Category
        {
            Name = categoryDto.Name,
            Status = categoryDto.Status,
        };

        if (categoryDto.ImageFile != null)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(categoryDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image upload failed");
            }

            category.ImageUrl = imageUrl;
        }

        await _productService.CreateCategoryAsync(category);

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    // Update an existing product category
    [HttpPut("category/{id}")]
    public async Task<IActionResult> UpdateCategory(string id, [FromForm] CategoryDto categoryDto)
    {
        var existingCategory = await _productService.GetCategoryByIdAsync(id);
        if (existingCategory == null)
        {
            return NotFound();
        }

        existingCategory.Name = categoryDto.Name;
        existingCategory.Status = categoryDto.Status;

        if (categoryDto.ImageFile != null)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(categoryDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image upload failed");
            }

            existingCategory.ImageUrl = imageUrl;
        }

        existingCategory.UpdatedAt = DateTime.Now;
        await _productService.UpdateCategoryAsync(id, existingCategory);

        return Ok(existingCategory);
    }

    // Product search with sorting by price and vendor rating
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string? name,
        [FromQuery] string? author,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? vendorId,
        [FromQuery] string? categoryId,
        [FromQuery] decimal? minRating,
        [FromQuery] decimal? maxRating,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true)
    {
        var products = await _productService.SearchProductsAsync(name, author, minPrice, maxPrice, vendorId, categoryId, minRating, maxRating, sortBy, isAscending);
        return Ok(products);
    }
}
