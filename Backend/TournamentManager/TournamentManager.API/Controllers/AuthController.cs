using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TournamentManager.API.Data;
using TournamentManager.API.DTOs;
using TournamentManager.API.Entities;

namespace TournamentManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists!");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Email = request.Email,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully!");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(user);
            return Ok(token);
        }

        //Create Token
        private string CreateToken(User user)
        {
            // STEP 1: The "Claims" (The Passport Details)
            // We create a list of facts about the user.
            // This data is NOT secret (anyone can decode it), so don't put passwords here!
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username), // Fact: "My name is Ali"
                new Claim(ClaimTypes.Role, user.Role),     // Fact: "I am an Admin"
                new Claim("UserId", user.Id.ToString())    // Fact: "My ID is 5" (Custom claim)
            };

            // STEP 2: The "Key" (The Official Stamp)
            // We fetch the secret string from appsettings.json.
            // This is the only thing that makes the token secure. If someone else has this key,
            // they can forge passports.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:Key").Value!));

            // STEP 3: The "Signature" (Anti-Forgery Tech)
            // We choose an algorithm (HmacSha512) to mix the Key + Claims together.
            // This creates a unique mathematical signature.
            // If a hacker changes "User" to "Admin" in the token, the signature breaks.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // STEP 4: The "Token Object" (Assembling the Passport)
            // We put it all together: Claims + Expiration + Signature.
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(14), //Resets Token every two weeks
                signingCredentials: creds
            );

            // STEP 5: Final Output (The Encoded String)
            // This turns the object into the long "eyJh..." string.
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
