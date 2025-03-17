using bike_project.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace bike_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BikeStores46Context _context;

        public LoginController(IConfiguration configuration, BikeStores46Context context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthDto authDto)
        {
            // Example: validate credentials (you should implement your actual validation logic here)
            //if (!IsValidUser(authDto.Email, authDto.Password))
            //{
            //    return Unauthorized(); // Or any appropriate status code for failed authentication
            //}
           
            var publisher = await _context.Staffs.FirstOrDefaultAsync(x => x.Email == authDto.Email);
            if (!BCrypt.Net.BCrypt.EnhancedVerify(authDto.Password, publisher.Password))
            {
                return BadRequest("Password not matching");
            }
            if (publisher == null)
            {
                return NotFound();
            }

            // Example: create claims (customize based on your application's requirements)
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, authDto.Email),
                // Add more claims as needed, e.g., roles, permissions, etc.
            };

            // Example: generate JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])), SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        // Example method to validate user credentials (replace with your actual logic)
        private bool IsValidUser(string email, string password)
        {
            // Example: perform validation against your data store or authentication service
            // Replace this with your actual implementation
            if (email == "example@email.com" && password == "password")
            {
                return true;
            }
            return false;
        }
    }
}
