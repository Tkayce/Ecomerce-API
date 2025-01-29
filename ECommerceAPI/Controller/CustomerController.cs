using ECommerceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controller
{
    public class CustomerController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }
        // 1. Get All Products
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // 2. Get Product Details by ID
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { Message = "Product not found." });

            return Ok(product);
        }

        // 3. Place an Order
        [HttpPost("orders")]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            if (order == null || !order.Products.Any())
                return BadRequest(new { Message = "Order must contain at least one product." });

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order placed successfully.", OrderId = order.Id });
        }

        // 4. Get All Orders for a Customer
        [HttpGet("orders/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Products)
                .ToListAsync();

            if (!orders.Any())
                return NotFound(new { Message = "No orders found for this customer." });

            return Ok(orders);
        }

        // 5. Get Order Details by ID
        [HttpGet("orders/details/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { Message = "Order not found." });

            return Ok(order);
        }
    }
}
