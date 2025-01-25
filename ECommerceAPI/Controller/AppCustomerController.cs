using ECommerceAPI.DTOs;
using ECommerceAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
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

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

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

       //Endpont for login
        [HttpPost("login")]
        public IActionResult Login(LoginDTO request)
        {
           
            var customer = _context.AppCustomers.FirstOrDefault(c => c.Email == request.Email);
            if (customer == null)
                return BadRequest("User not found.");

           
            if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                return BadRequest("Invalid password.");

            // Ideally, generate and return JWT (we'll implement this later)
            return Ok("Login successful.");
        }

        // Profile Management Endpoint
        [HttpGet("profile/{id}")]
        public IActionResult GetProfile(int id)
        {
            var customer = _context.AppCustomers.Find(id);
            if (customer == null)
                return NotFound("Customer not found.");

            return Ok(new
            {
                customer.Id,
                customer.Name,
                customer.Email
            });
        }
         
        [HttpPut("profile/{id}")]
        public IActionResult UpdateProfile(int id, UpdateAppCostumerDTO request)
        {
            var customer = _context.AppCustomers.Find(id);
            if (customer == null)
                return NotFound("Customer not found.");

            if (!string.IsNullOrEmpty(request.Name))
                customer.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Email))
            {
                if (_context.AppCustomers.Any(c => c.Email == request.Email && c.Id != id))
                    return BadRequest("Email already in use.");
                customer.Email = request.Email;
            }

            _context.SaveChanges();

            return Ok("Profile updated successfully.");
        }


    }
}
