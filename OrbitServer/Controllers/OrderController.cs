// File Name: OrderController.cs
// Description: Handles all order, order item, shoping cart related business logic

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly UserService _userService;
    private readonly ProductService _productService;
    private readonly NotificationService _notificationService;

    public OrderController(OrderService orderService, UserService userService, ProductService productService, NotificationService notificationService)
    {
        _orderService = orderService;
        _userService = userService;
        _productService = productService;
        _notificationService = notificationService;
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

        var customerCart = await _orderService.GetCartByCustomerIdAsync(customer.Id);
        if (customerCart != null)
        {
            return BadRequest("Customer already has items in shopping cart");
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

        if (cart.Address != null)
        {
            existingCart.Address = cart.Address;
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

    // Get all orders
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    // Get all orders by status
    [HttpGet("pending")]
    public async Task<IActionResult> GetOrdersByPendingStatus()
    {
        var orders = await _orderService.GetOrdersByStatusAsync(OrderStatus.Pending);
        return Ok(orders);
    }

    // Get order by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    // Get order by customer id
    [HttpGet("customer/{id}")]
    public async Task<IActionResult> GetOrderByCustomerId(string id)
    {
        var orders = await _orderService.GetOrdersByCustomerIdAsync(id);
        if (orders == null)
        {
            return NotFound();
        }
        return Ok(orders);
    }

    // Create a new order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Cart cart)
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

        var orderItems = new List<OrderItem>();

        foreach (CartItem cartItem in cart.CartItems)
        {
            if (cartItem.Product == null || cartItem.Product.Id == null)
            {
                return BadRequest("Product id is missing");
            }

            var product = await _productService.GetProductByIdAsync(cartItem.Product.Id);

            if (product == null)
            {
                return BadRequest("Product is not available");
            }

            if (product.Stock >= cartItem.Quantity)
            {
                product.Stock = product.Stock - cartItem.Quantity;

                await _productService.UpdateProductAsync(product.Id, product);

                if (product.Stock < 10)
                {
                    var notification = new Notification
                    {
                        Title = $"{product.Name} running out of stock",
                        Body = $"{product.Name} only has {product.Stock} items remaining. Please make sure to restock.",
                        User = product.Vendor,
                        SeenStatus = false,
                    };

                    await _notificationService.CreateNotificationAsync(notification);

                    var adminUsers = await _userService.GetUsersByRoleAsync("Admin");
                    foreach (User admin in adminUsers)
                    {
                        var adminNotification = new Notification
                        {
                            Title = $"{product.Name} running out of stock",
                            Body = $"{product.Name} only has {product.Stock} items remaining. Please make sure to restock.",
                            User = admin,
                            SeenStatus = false,
                        };

                        await _notificationService.CreateNotificationAsync(adminNotification);
                    }
                }
            }
            else
            {
                return BadRequest($"Not enough stock for product {product.Name}. Available stock: {product.Stock}");
            }

            var orderItem = new OrderItem
            {
                Product = product,
                Quantity = cartItem.Quantity,
                TotalPrice = cartItem.TotalPrice,
                Vendor = product.Vendor,
                Customer = customer,
                Status = OrderStatus.Pending,
            };

            await _orderService.CreateOrderItemAsync(orderItem);

            orderItems.Add(orderItem);
        }

        var order = new Order
        {
            OrderItems = orderItems,
            Customer = customer,
            OrderPrice = cart.CartPrice,
            CancelRequest = false,
            Status = OrderStatus.Pending,
        };

        if (cart.Address != null)
        {
            order.Address = cart.Address;
        }

        await _orderService.CreateOrderAsync(order);

        foreach (OrderItem dbOrderItem in order.OrderItems)
        {
            dbOrderItem.OrderId = order.Id;

            await _orderService.UpdateOrderItemAsync(dbOrderItem.Id, dbOrderItem);
        }

        await _orderService.DeleteCartAsync(cart.Id);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    // Update an existing order
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] Order order)
    {
        var existingOrder = await _orderService.GetOrderByIdAsync(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        if (order.Status != null && order.Status != existingOrder.Status)
        {

            existingOrder.Status = order.Status;

            if (order.Status == OrderStatus.Delivered)
            {
                var notification = new Notification
                {
                    Title = $"Order: {existingOrder.Id} is Delivered",
                    Body = $"Order: {existingOrder.Id} is delivered. Thank you for shopping with us!",
                    User = existingOrder.Customer,
                    SeenStatus = false,
                };

                await _notificationService.CreateNotificationAsync(notification);

                foreach (OrderItem orderItem in existingOrder.OrderItems)
                {
                    var deliveredOrderItem = await _orderService.GetOrderItemByIdAsync(orderItem.Id);
                    deliveredOrderItem.Status = OrderStatus.Delivered;
                    deliveredOrderItem.UpdatedAt = DateTime.Now;
                    await _orderService.UpdateOrderItemAsync(deliveredOrderItem.Id, deliveredOrderItem);
                }
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                var notification = new Notification
                {
                    Title = $"Order: {existingOrder.Id} is Cancelled",
                    Body = $"Order: {existingOrder.Id} is cancelled due to the reason given as: {existingOrder.CancelReason}",
                    User = existingOrder.Customer,
                    SeenStatus = false,
                };

                await _notificationService.CreateNotificationAsync(notification);

                foreach (OrderItem orderItem in existingOrder.OrderItems)
                {
                    var dbOrderItem = await _orderService.GetOrderItemByIdAsync(orderItem.Id);
                    dbOrderItem.Status = OrderStatus.Cancelled;
                    dbOrderItem.UpdatedAt = DateTime.Now;
                    await _orderService.UpdateOrderItemAsync(dbOrderItem.Id, dbOrderItem);
                }

            }
        }

        if (order.Address != null)
        {
            existingOrder.Address = order.Address;
        }

        existingOrder.UpdatedAt = DateTime.Now;
        await _orderService.UpdateOrderAsync(id, existingOrder);

        return Ok(existingOrder);
    }

    // Update an existing order status
    [HttpPut("status/{id}")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromQuery, Required] OrderStatus status)
    {
        var existingOrder = await _orderService.GetOrderByIdAsync(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        existingOrder.Status = status;

        existingOrder.UpdatedAt = DateTime.Now;
        await _orderService.UpdateOrderAsync(id, existingOrder);

        if (status == OrderStatus.Delivered)
        {
            var notification = new Notification
            {
                Title = $"Order: {existingOrder.Id} is Delivered",
                Body = $"Order: {existingOrder.Id} is delivered. Thank you for shopping with us!",
                User = existingOrder.Customer,
                SeenStatus = false,
            };

            await _notificationService.CreateNotificationAsync(notification);

            foreach (OrderItem orderItem in existingOrder.OrderItems)
            {
                var deliveredOrderItem = await _orderService.GetOrderItemByIdAsync(orderItem.Id);
                deliveredOrderItem.Status = OrderStatus.Delivered;
                deliveredOrderItem.UpdatedAt = DateTime.Now;
                await _orderService.UpdateOrderItemAsync(deliveredOrderItem.Id, deliveredOrderItem);
            }
        }

        if (status == OrderStatus.Cancelled)
        {
            var notification = new Notification
            {
                Title = $"Order: {existingOrder.Id} is Cancelled",
                Body = $"Order: {existingOrder.Id} is cancelled due to the reason given as: {existingOrder.CancelReason}",
                User = existingOrder.Customer,
                SeenStatus = false,
            };

            await _notificationService.CreateNotificationAsync(notification);

            foreach (OrderItem orderItem in existingOrder.OrderItems)
            {
                var dbOrderItem = await _orderService.GetOrderItemByIdAsync(orderItem.Id);
                dbOrderItem.Status = OrderStatus.Cancelled;
                dbOrderItem.UpdatedAt = DateTime.Now;
                await _orderService.UpdateOrderItemAsync(dbOrderItem.Id, dbOrderItem);
            }

        }

        return Ok(existingOrder);
    }

    // Delete order by id
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        var existingOrder = await _orderService.GetOrderByIdAsync(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        await _orderService.DeleteOrderAsync(id);
        return Ok($"Successfully deleted order with id: {id}");
    }

    // Cancel order request
    [HttpPut("cancel/{id}")]
    public async Task<IActionResult> CancelOrderRequest(string id, [FromQuery, Required] string cancelReason)
    {
        var existingOrder = await _orderService.GetOrderByIdAsync(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        if (existingOrder.Status != OrderStatus.Pending)
        {
            return BadRequest("Order is already dispatched! Cannot request to cancel the order");
        }

        existingOrder.CancelRequest = true;
        existingOrder.CancelReason = cancelReason;

        var csrUsers = await _userService.GetUsersByRoleAsync("CSR");
        foreach (User csr in csrUsers)
        {
            var notification = new Notification
            {
                Title = $"Order cancel request for Order: {existingOrder.Id}",
                Body = $"Order: {existingOrder.Id} is requested to be cancelled due to the reason given as: {existingOrder.CancelReason}",
                User = csr,
                SeenStatus = false,
            };

            await _notificationService.CreateNotificationAsync(notification);
        }

        existingOrder.UpdatedAt = DateTime.Now;
        await _orderService.UpdateOrderAsync(id, existingOrder);

        return Ok(existingOrder);
    }

    // CSR cancel order
    [HttpPut("csr/cancel/{id}")]
    public async Task<IActionResult> CsrCancelOrderRequest(string id, [FromQuery, Required] string cancelReason)
    {
        var existingOrder = await _orderService.GetOrderByIdAsync(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        if (existingOrder.Status != OrderStatus.Pending)
        {
            return BadRequest($"Cannot cancel the order as the order is in {existingOrder.Status}");
        }

        existingOrder.Status = OrderStatus.Cancelled;
        existingOrder.CsrCancelReason = cancelReason;

        {
            var notification = new Notification
            {
                Title = $"Order: {existingOrder.Id} is Cancelled",
                Body = $"Order: {existingOrder.Id} is cancelled due to the customer reason given as: {existingOrder.CancelReason}. CSR feedback: {existingOrder.CsrCancelReason}",
                User = existingOrder.Customer,
                SeenStatus = false,
            };

            await _notificationService.CreateNotificationAsync(notification);

            foreach (OrderItem orderItem in existingOrder.OrderItems)
            {
                var dbOrderItem = await _orderService.GetOrderItemByIdAsync(orderItem.Id);
                dbOrderItem.Status = OrderStatus.Cancelled;
                dbOrderItem.UpdatedAt = DateTime.Now;
                await _orderService.UpdateOrderItemAsync(dbOrderItem.Id, dbOrderItem);
            }

        }

        existingOrder.UpdatedAt = DateTime.Now;
        await _orderService.UpdateOrderAsync(id, existingOrder);

        return Ok(existingOrder);
    }

    // Get all cancel request orders
    [HttpGet("cancel/requests")]
    public async Task<IActionResult> GetCancelRequestOrders()
    {
        var orders = await _orderService.GetCancelRequestOrdersAsync();
        if (orders == null)
        {
            return NotFound();
        }
        return Ok(orders);
    }

    // Get all order items by customer id
    [HttpGet("item/customer/{id}")]
    public async Task<IActionResult> GetOrderItemsByCustomerId(string id)
    {
        var orderItems = await _orderService.GetOrderItemsByCustomerIdAsync(id);
        if (orderItems == null)
        {
            return NotFound();
        }
        return Ok(orderItems);
    }

    // Get all order items by vendor id
    [HttpGet("item/vendor/{id}")]
    public async Task<IActionResult> GetOrderItemsByVendorId(string id)
    {
        var orderItems = await _orderService.GetOrderItemsByVendorIdAsync(id);
        if (orderItems == null)
        {
            return NotFound();
        }
        return Ok(orderItems);
    }

    // Update order item status
    [HttpPut("item/status/{id}")]
    public async Task<IActionResult> UpdateOrderItemStatus(string id, [FromQuery, Required] OrderStatus status)
    {
        var existingOrderItem = await _orderService.GetOrderItemByIdAsync(id);
        if (existingOrderItem == null)
        {
            return NotFound();
        }

        existingOrderItem.Status = status;

        existingOrderItem.UpdatedAt = DateTime.Now;
        await _orderService.UpdateOrderItemAsync(id, existingOrderItem);

        var order = await _orderService.GetOrderByIdAsync(existingOrderItem.OrderId);
        bool isDelivered = true;

        if (status == OrderStatus.Dispatched)
        {
            isDelivered = false;
        }

        if (status == OrderStatus.Delivered)
        {
            var notification = new Notification
            {
                Title = $"Order Item: {existingOrderItem.Id} is Delivered",
                Body = $"Order Item: {existingOrderItem.Id} is delivered. Enjoy the item! Feel free to rate the vendor.",
                User = existingOrderItem.Customer,
                SeenStatus = false,
            };

            await _notificationService.CreateNotificationAsync(notification);
        }

        if (order != null && status == OrderStatus.Delivered)
        {
            foreach (OrderItem orderItem in order.OrderItems)
            {
                var dbOrderItem = await _orderService.GetOrderItemByIdAsync(orderItem.Id);
                if (dbOrderItem.Status != OrderStatus.Delivered)
                {
                    isDelivered = false;
                }
            }
            order.Status = OrderStatus.Partial_Delivered;
            order.UpdatedAt = DateTime.Now;
            await _orderService.UpdateOrderAsync(order.Id, order);
        }

        if (order != null && isDelivered == true)
        {
            order.Status = OrderStatus.Delivered;
            order.UpdatedAt = DateTime.Now;
            await _orderService.UpdateOrderAsync(order.Id, order);

            var notification = new Notification
            {
                Title = $"Order: {order.Id} is Delivered",
                Body = $"Order: {order.Id} is delivered. Thank you for shopping with us!",
                User = order.Customer,
                SeenStatus = false,
            };

            await _notificationService.CreateNotificationAsync(notification);
        }

        return Ok(existingOrderItem);
    }
}
