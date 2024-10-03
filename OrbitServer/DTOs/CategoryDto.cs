// File Name: Category.cs
// Description: Request body template for category creation and update

public class CategoryDto
{
    public string Name { get; set; }
    public bool Status { get; set; }
    public IFormFile? ImageFile { get; set; }
}