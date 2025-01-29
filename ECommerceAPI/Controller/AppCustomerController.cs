using ECommerceAPI.DTOs;
using ECommerceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controller
{
    public class AppCustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AppCustomerController(ApplicationDbContext context)

        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AppCustomerDTO request)
        {
            if (await _context.AppCustomers.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest("Email already exists.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password); // Hash the password

            var appCustomer = new AppCustomer
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hashedPassword
            };

            _context.AppCustomers.Add(appCustomer);
            await _context.SaveChangesAsync();
            return Ok("Registration successful.");
        }

        // User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            var customer = await _context.AppCustomers.FirstOrDefaultAsync(c => c.Email == request.Email);
            if (customer == null)
                return BadRequest("User not found.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash)) // Verify the password
                return BadRequest("Invalid password.");

            return Ok("Login successful.");
        }

        // Get Customer Profile
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var customer = await _context.AppCustomers.FindAsync(id);
            if (customer == null)
                return NotFound("Customer not found.");

            return Ok(new
            {
                customer.Id,
                customer.Name,
                customer.Email
            });
        }

        // Update Customer Profile
        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, AppCustomerDTO request)
        {
            var customer = await _context.AppCustomers.FindAsync(id);
            if (customer == null)
                return NotFound("Customer not found.");

            if (!string.IsNullOrEmpty(request.Name))
                customer.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Email))
            {
                if (await _context.AppCustomers.AnyAsync(c => c.Email == request.Email && c.Id != id))
                    return BadRequest("Email already in use.");

                customer.Email = request.Email;
            }

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully.");
        }

        // Delete Customer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.AppCustomers.FindAsync(id);
            if (customer == null)
                return NotFound("Customer not found.");

            _context.AppCustomers.Remove(customer);
            await _context.SaveChangesAsync();
            return Ok("Customer deleted successfully.");
        }
    }
}


  