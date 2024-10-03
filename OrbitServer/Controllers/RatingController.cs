// File Name: RatingController.cs
// Description: Handles all vendor rating related business logic

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class RatingController : ControllerBase
{
    private readonly RatingService _ratingService;
    private readonly UserService _userService;

    public RatingController(RatingService ratingService, UserService userService)
    {
        _ratingService = ratingService;
        _userService = userService;
    }

    // Get all vendor ratings
    [HttpGet]
    public async Task<IActionResult> GetVendorRatings()
    {
        var ratings = await _ratingService.GetVendorRatingsAsync();
        return Ok(ratings);
    }

    // Get vendor rating by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVendorRating(string id)
    {
        var rating = await _ratingService.GetVendorRatingByIdAsync(id);
        if (rating == null)
        {
            return NotFound();
        }
        return Ok(rating);
    }

    // Create a new vendor rating
    [HttpPost]
    public async Task<IActionResult> CreateVendorRating([FromBody] VendorRating vendorRating)
    {
        if (vendorRating == null)
        {
            return BadRequest("Vendor Rating data is missing.");
        }

        var customer = await _userService.GetUserByIdAsync(vendorRating.Customer.Id);
        if (customer == null)
        {
            return BadRequest($"Customer with id: {vendorRating.Customer.Id} does not exist");
        }

        var vendor = await _userService.GetUserByIdAsync(vendorRating.Vendor.Id);
        if (vendor == null)
        {
            return BadRequest($"Vendor with id: {vendorRating.Vendor.Id} does not exist");
        }

        var existingRating = await _ratingService.GetVendorRatingByCustomerAndVendorIdAsync(customer.Id, vendor.Id);
        if (existingRating != null)
        {
            return BadRequest($"{customer.Name}, You have already rated this vendor");
        }

        vendor.RatingCount++;
        vendor.Rating = Math.Round((vendor.Rating + vendorRating.Rating) / vendor.RatingCount, 2);
        vendor.UpdatedAt = DateTime.Now;
        await _userService.UpdateUserAsync(vendor.Id, vendor);

        vendorRating.Vendor = vendor;
        await _ratingService.CreateVendorRatingAsync(vendorRating);

        return CreatedAtAction(nameof(GetVendorRating), new { id = vendorRating.Id }, vendorRating);
    }

    // Update an existing vendor rating
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendorRating(string id, [FromQuery, Required] string comment)
    {
        var existingVendorRating = await _ratingService.GetVendorRatingByIdAsync(id);
        if (existingVendorRating == null)
        {
            return BadRequest("Vendor rating id provided is incorrect");
        }

        existingVendorRating.Comment = comment;
        existingVendorRating.UpdatedAt = DateTime.Now;
        await _ratingService.UpdateVendorRatingAsync(id, existingVendorRating);

        return Ok(existingVendorRating);
    }

    // Delete vendor rating by id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendorRating(string id)
    {
        var existingVendorRating = await _ratingService.GetVendorRatingByIdAsync(id);
        if (existingVendorRating == null)
        {
            return NotFound();
        }

        await _ratingService.DeleteVendorRatingAsync(id);
        return Ok($"Successfully deleted vendor rating with id: {id}");
    }

    // Get vendor ratings by vendor id
    [HttpGet("vendor/{id}")]
    public async Task<IActionResult> GetVendorRatingByVendorId(string id)
    {
        var vendor = await _userService.GetUserByIdAsync(id);
        if (vendor == null)
        {
            return BadRequest($"Vendor with id: {id} does not exist");
        }

        var ratings = await _ratingService.GetVendorRatingsByVendorIdAsync(id);
        if (ratings == null)
        {
            return NotFound();
        }
        return Ok(ratings);
    }

    // Get vendor ratings by customer id
    [HttpGet("customer/{id}")]
    public async Task<IActionResult> GetVendorRatingByCustomerId(string id)
    {
        var customer = await _userService.GetUserByIdAsync(id);
        if (customer == null)
        {
            return BadRequest($"Customer with id: {id} does not exist");
        }

        var ratings = await _ratingService.GetVendorRatingsByCustomerIdAsync(id);
        if (ratings == null)
        {
            return NotFound();
        }
        return Ok(ratings);
    }

    // Get vendor ratings by customer and vendor id
    [HttpGet("customer/vendor")]
    public async Task<IActionResult> GetVendorRatingByCustomerAndVendorId([FromQuery, Required] string customerId, [FromQuery, Required] string vendorId)
    {
        var customer = await _userService.GetUserByIdAsync(customerId);
        if (customer == null)
        {
            return BadRequest($"Customer with id: {customerId} does not exist");
        }

        var vendor = await _userService.GetUserByIdAsync(vendorId);
        if (vendor == null)
        {
            return BadRequest($"Vendor with id: {vendorId} does not exist");
        }

        var rating = await _ratingService.GetVendorRatingByCustomerAndVendorIdAsync(customerId, vendorId);
        if (rating == null)
        {
            return NotFound();
        }
        return Ok(rating);
    }
}
