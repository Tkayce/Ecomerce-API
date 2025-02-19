using ECommerceAPI.DTOs;
using ECommerceAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ECommerceAPI.Model.OrderProduct;

namespace ECommerceAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("place-order/{cartId}")]
        public async Task<IActionResult> PlaceOrder(int cartId)
        {
            // Retrieve the Cart with its CartItems and their associated Product information
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == cartId && !c.IsCheckedOut);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest(new { Message = "Cart is empty or does not exist." });

            // Now retrieve the customer directly from the cart
            var customer = await _context.Customers.FindAsync(cart.CustomerId);
            if (customer == null)
                return BadRequest(new { Message = "Invalid CustomerId. Customer does not exist." });

            // Create the order from the cart data
            var order = new Order
            {
                CustomerId = cart.CustomerId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending",
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                OrderProduct = cart.CartItems.Select(ci => new OrderProduct
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // Mark the cart as checked out so it won't be reused
            cart.IsCheckedOut = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order placed successfully.", OrderId = order.Id });
        }


        // Get All Orders for a Customer
        [HttpGet("customer/{customerId}/orders")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderProduct)
                .ThenInclude(op => op.Product)
                .ToListAsync();

            if (!orders.Any())
                return NotFound(new { Message = "No orders found for this customer." });

            var orderList = orders.Select(order => new
            {
                OrderId = order.Id,
                TotalItems = order.OrderProduct.Sum(op => op.Quantity),
                TotalAmount = order.OrderProduct.Sum(op => op.Price * op.Quantity),
                OrderStatus = order.OrderStatus,
                Items = order.OrderProduct.Select(op => new
                {
                    op.ProductId,
                    ProductName = op.Product.Name,
                    op.Quantity,
                    op.Price,
                    TotalPrice = op.Price * op.Quantity
                }).ToList()
            }).ToList();

            return Ok(orderList);
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] UpdateOrderRequest request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProduct)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { Message = "Order not found." });

            // Update fields as necessary; here we update OrderStatus as an example
            order.OrderStatus = request.OrderStatus ?? order.OrderStatus;

            // If needed, update other fields here

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order updated successfully." });
        }

        // Cancel Order
        [HttpDelete("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderProduct).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound(new { Message = "Order not found." });

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order has been canceled successfully." });
        }
    }
}
