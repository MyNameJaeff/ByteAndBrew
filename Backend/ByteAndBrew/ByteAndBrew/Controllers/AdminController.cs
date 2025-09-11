using ByteAndBrew.Data;
using ByteAndBrew.Dtos.Admin;
using ByteAndBrew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ByteAndBrew.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        private readonly IConfiguration _configuration;

        public AdminsController(ByteAndBrewDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var admins = await _db.Admins
                .Select(a => new AdminReadDto
                {
                    Id = a.Id,
                    Username = a.Username
                })
                .ToListAsync();

            return Ok(admins);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            var admin = await _db.Admins
                .Where(a => a.Id == id)
                .Select(a => new AdminReadDto
                {
                    Id = a.Id,
                    Username = a.Username
                })
                .FirstOrDefaultAsync();

            if (admin == null) return NotFound($"There's no admin with id {id}");
            return Ok(admin);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AdminCreateDto dto)
        {
            var existingAdmin = await _db.Admins
                .FirstOrDefaultAsync(a => a.Username == dto.Username);
            if (existingAdmin != null)
                return BadRequest("Admin with this username already exists.");

            // Validate password strength (optional but recommended)
            if (dto.Password.Length < 6)
                return BadRequest("Password must be at least 6 characters long.");

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var admin = new Admin
            {
                Username = dto.Username,
                PasswordHash = hashedPassword
            };

            _db.Admins.Add(admin);
            await _db.SaveChangesAsync();

            var readDto = new AdminReadDto
            {
                Id = admin.Id,
                Username = admin.Username
            };

            return CreatedAtAction(nameof(Get), new { id = admin.Id }, readDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, AdminUpdateDto dto)
        {
            var admin = await _db.Admins.FindAsync(id);
            if (admin == null) return NotFound($"There's no admin with id {id}");

            // Check if username already exists (excluding current admin)
            var existingAdmin = await _db.Admins
                .FirstOrDefaultAsync(a => a.Username == dto.Username && a.Id != id);
            if (existingAdmin != null)
                return BadRequest("Admin with this username already exists.");

            admin.Username = dto.Username;

            // Hash new password only if provided
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 6)
                    return BadRequest("Password must be at least 6 characters long.");

                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _db.SaveChangesAsync();

            var readDto = new AdminReadDto
            {
                Id = admin.Id,
                Username = admin.Username
            };

            return Ok(readDto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            var admin = await _db.Admins.FindAsync(id);
            if (admin == null) return NotFound($"There's no admin with id {id}");

            // Prevent deletion of the last admin
            var adminCount = await _db.Admins.CountAsync();
            if (adminCount <= 1)
                return BadRequest("Cannot delete the last admin. There must always be at least one admin.");

            _db.Admins.Remove(admin);
            await _db.SaveChangesAsync();

            return Ok($"The admin with id {id} has been deleted");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(AdminLoginDto dto)
        {
            System.Diagnostics.Debug.WriteLine($"Received DTO: Username='{dto.Username}', Password='{dto.Password}'");
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");

            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Username == dto.Username);
            if (admin == null) return Unauthorized("Invalid credentials");

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);
            if (!isValid) return Unauthorized("Invalid credentials");

            // Generate JWT
            var jwt = GenerateJwtToken(admin);

            return Ok(new
            {
                token = jwt,
                adminId = admin.Id,
                username = admin.Username,
                expiresAt = DateTime.UtcNow.AddHours(2)
            });
        }

        private string GenerateJwtToken(Admin admin)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                ?? Environment.GetEnvironmentVariable("JWT_KEY")
                ?? throw new InvalidOperationException("JWT key not configured"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, admin.Username),
                    new Claim("AdminId", admin.Id.ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Audience = _configuration["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}