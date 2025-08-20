using Byte___Brew.Data;
using Byte___Brew.Dtos.Admin;
using Byte___Brew.Dtos.NewFolder;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Byte___Brew.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public AdminsController(ByteAndBrewDbContext db) => _db = db;

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

        [Authorize]
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

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(AdminCreateDto dto)
        {
            var existingAdmin = await _db.Admins
                .FirstOrDefaultAsync(a => a.Username == dto.Username);
            if (existingAdmin != null) return BadRequest("Admin with this username already exists.");

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

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

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, AdminCreateDto dto)
        {
            var admin = await _db.Admins.FindAsync(id);
            if (admin == null) return NotFound($"There's no admin with id {id}");

            admin.Username = dto.Username;

            // Hash new password only if a new one is provided
            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);

            await _db.SaveChangesAsync();

            var readDto = new AdminReadDto
            {
                Id = admin.Id,
                Username = admin.Username
            };

            return Ok(readDto);
        }


        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Admins.FindAsync(id);
            if (t == null) return NotFound($"There's no admin with id {id}");
            _db.Admins.Remove(t);
            await _db.SaveChangesAsync();
            return Ok($"The admin with id {id} has been deleted");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(AdminLoginDto dto)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Username == dto.Username);
            if (admin == null) return Unauthorized("Invalid credentials");

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);
            if (!isValid) return Unauthorized("Invalid credentials");

            // Generate JWT
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, admin.Username),
                    new Claim("AdminId", admin.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Issuer"],
                Audience = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Ok(new { token = jwt });
        }
    }
}
