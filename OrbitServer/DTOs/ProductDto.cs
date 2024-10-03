// File Name: ProductDto.cs
// Description: Request body template for product creation and update

public class ProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string CategoryId { get; set; }
    public string? VendorId { get; set; }
}
