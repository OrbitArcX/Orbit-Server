using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly UserService _userService;
    private readonly ProductService _productService;

    public OrderController(OrderService orderService, UserService userService, ProductService productService)
    {
        _orderService = orderService;
        _userService = userService;
        _productService = productService;
    }

    // Get all carts
    [HttpGet("cart")]
    public async Task<IActionResult> GetCarts()
    {
        var carts = await _orderService.GetCartsAsync();
        return Ok(carts);
    }

    // Get cart by id
    [HttpGet("cart/{id}")]
    public async Task<IActionResult> GetCart(string id)
    {
        var cart = await _orderService.GetCartByIdAsync(id);
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    // Get cart by customer id
    [HttpGet("cart/customer/{id}")]
    public async Task<IActionResult> GetCartByCustomerId(string id)
    {
        var cart = await _orderService.GetCartByCustomerIdAsync(id);
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    // Create a new cart
    [HttpPost("cart")]
    public async Task<IActionResult> CreateUser([FromBody] Cart cart)
    {
        if (cart == null)
        {
            return BadRequest("Cart data is missing");
        }

        if (cart.CartItems == null)
        {
            return BadRequest("Cart items are missing");
        }

        if (cart.Customer == null || cart.Customer.Id == null)
        {
            return BadRequest("Customer details are missing");
        }

        var customer = await _userService.GetUserByIdAsync(cart.Customer.Id);
        if (customer == null)
        {
            return BadRequest("Customer does not exist");
        }

        foreach (CartItem cartItem in cart.CartItems)
        {
            if (cartItem.Product == null || cartItem.Product.Id == null)
            {
                return BadRequest("Product id is missing");
            }

            var product = await _productService.GetProductByIdAsync(cartItem.Product.Id);
            cartItem.Product = product;
        }

        cart.Customer = customer;
        await _orderService.CreateCartAsync(cart);

        return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, cart);
    }

    // Update an existing cart
    [HttpPut("cart/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] Cart cart)
    {
        var existingCart = await _orderService.GetCartByIdAsync(id);
        if (existingCart == null)
        {
            return NotFound();
        }

        if (cart.CartItems != null)
        {
            foreach (CartItem cartItem in cart.CartItems)
            {
                if (cartItem.Product == null || cartItem.Product.Id == null)
                {
                    return BadRequest("Product id is missing");
                }

                var product = await _productService.GetProductByIdAsync(cartItem.Product.Id);
                cartItem.Product = product;
            }
        }

        existingCart.CartItems = cart.CartItems;
        existingCart.CartPrice = cart.CartPrice;

        existingCart.UpdatedAt = DateTime.Now;
        await _orderService.UpdateCartAsync(id, existingCart);

        return Ok(existingCart);
    }

    // Delete cart by id
    [HttpDelete("cart/{id}")]
    public async Task<IActionResult> DeleteCart(string id)
    {
        var existingCart = await _orderService.GetCartByIdAsync(id);
        if (existingCart == null)
        {
            return NotFound();
        }

        await _orderService.DeleteCartAsync(id);
        return Ok($"Successfully deleted cart with id: {id}");
    }
}
