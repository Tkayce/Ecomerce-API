using ECommerceAPI.DTOs;
using ECommerceAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        // ✅ Add item to cart
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
                return NotFound(new { Message = "Product not found" });

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && !c.IsCheckedOut);

            if (cart == null)
            {
                cart = new Cart { CustomerId = request.CustomerId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = product.Price
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Item added to cart successfully", CartId = cart.Id });
        }

        // ✅ Get all items in a customer's cart
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCartItems(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

            if (cart == null || !cart.CartItems.Any())
                return NotFound(new { Message = "Cart is empty." });

            var cartItems = cart.CartItems.Select(ci => new
            {
                ci.ProductId,
                ProductName = ci.Product.Name,
                ci.Quantity,
                ci.Price,
                TotalPrice = ci.Price * ci.Quantity
            }).ToList();

            var totalAmount = cartItems.Sum(ci => ci.TotalPrice);
            var totalItems = cartItems.Sum(ci => ci.Quantity);

            return Ok(new
            {
                TotalItems = totalItems,
                TotalAmount = totalAmount,
                CartItems = cartItems
            });
        }

        // ✅ Update quantity of an item in cart
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] AddToCartRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && !c.IsCheckedOut);

            if (cart == null)
                return NotFound(new { Message = "Cart not found" });

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (cartItem == null)
                return NotFound(new { Message = "Item not found in cart" });

            cartItem.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cart item updated successfully" });
        }

        // ✅ Delete a specific item from cart
        [HttpDelete("delete/{customerId}/{productId}")]
        public async Task<IActionResult> DeleteCartItem(int customerId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

            if (cart == null)
                return NotFound(new { Message = "Cart not found" });

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return NotFound(new { Message = "Item not found in cart" });

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Item removed from cart successfully" });
        }

        // ✅ Remove all items from cart
        [HttpDelete("clear/{customerId}")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

            if (cart == null)
                return NotFound(new { Message = "Cart not found" });

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Cart cleared successfully" });
        }

        // ✅ Place an order (checkout)
        [HttpPost("checkout/{customerId}")]
        public async Task<IActionResult> Checkout(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest(new { Message = "Cart is empty. Cannot place an order." });

            cart.IsCheckedOut = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Order placed successfully!", OrderId = cart.Id });
        }
    }
}
