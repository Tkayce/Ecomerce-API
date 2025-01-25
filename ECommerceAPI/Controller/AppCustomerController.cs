using ECommerceAPI.DTOs;
using ECommerceAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        [AllowAnonymous]
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
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO request)
        {
            // Simulated user validation (use database in production)
            if (request.Email == "test@example.com" && request.Password == "password")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("YourSecretKey");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.Name, "testUser"),
                new Claim(ClaimTypes.Role, "User") // Optional: add roles
            }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new { Token = tokenString });
            }

            return Unauthorized();
        }

        // Fetch User by Email
        [Authorize]
        [HttpGet("get-by-email")]
        public IActionResult GetUserByEmail(string email)
        {
            // Find user by email
            var user = _context.AppCustomers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Return user data
            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.PasswordHash // I will remove this in production stage!
            });
        }

        // Profile Management Endpoint
        [AllowAnonymous]
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
        [AllowAnonymous]
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
