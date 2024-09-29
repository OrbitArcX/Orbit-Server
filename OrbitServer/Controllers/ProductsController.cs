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

    // Create a new product with image upload
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromForm] ProductDto createProductDto)
    {
        if (createProductDto == null || createProductDto.ImageFile == null)
        {
            return BadRequest("Product data or image file is missing.");
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Stock = createProductDto.Stock,
        };

        var imageUrl = await _cloudinaryService.UploadImageAsync(createProductDto.ImageFile);
        if (string.IsNullOrEmpty(imageUrl))
        {
            return StatusCode(500, "Image upload failed.");
        }

        product.ImageUrl = imageUrl;

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

        existingProduct.Name = productDto.Name;
        existingProduct.Description = productDto.Description;
        existingProduct.Price = productDto.Price;
        existingProduct.Stock = productDto.Stock;

        if (productDto.ImageFile != null)
        {
            var imageUrl = await _cloudinaryService.UploadImageAsync(productDto.ImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, "Image upload failed.");
            }

            existingProduct.ImageUrl = imageUrl;
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

        await _productService.DeleteProductAsync(id);
        return Ok($"Successfully deleted product with id: {id}");
    }
}
