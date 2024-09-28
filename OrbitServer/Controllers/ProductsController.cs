using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly CloudinaryService _cloudinaryService;

    public ProductsController(ProductService productService, CloudinaryService cloudinaryService)
    {
        _productService = productService;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _productService.GetProductsAsync();
        return Ok(products);
    }

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

    // POST: Create a new product with image upload
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDto createProductDto)
    {
        if (createProductDto == null || createProductDto.ImageFile == null)
        {
            return BadRequest("Product data or image file is missing.");
        }

        // Map DTO to Product entity
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
        };

        // Upload the image to Cloudinary and get the URL
        var imageUrl = await _cloudinaryService.UploadImageAsync(createProductDto.ImageFile);
        if (string.IsNullOrEmpty(imageUrl))
        {
            return StatusCode(500, "Image upload failed.");
        }

        // Assign the Cloudinary URL to the product
        product.ImageUrl = imageUrl;

        // Save the product to MongoDB
        await _productService.CreateProductAsync(product);

        // Return a 201 Created response
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: Update an existing product with an optional image update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromForm] ProductDto productDto)
    {
        // Fetch the existing product
        var existingProduct = await _productService.GetProductByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Update the existing product's properties from the DTO
        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;
        existingProduct.Stock = productDto.Stock;

        // If a new image file is provided, upload it to Cloudinary
        if (productDto.ImageFile != null)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(productDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image upload failed.");
            }

            // Assign the new Cloudinary URL to the existing product
            existingProduct.ImageUrl = imageUrl;
        }

        // Update the product in MongoDB
        
        existingProduct.UpdatedAt = DateTime.Now;
        await _productService.UpdateProductAsync(id, existingProduct);

        return Ok(existingProduct);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var existingProduct = await _productService.GetProductByIdAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        await _productService.DeleteProductAsync(id);
        return Ok($"Successfully deleted product with id: {id}");
    }
}
