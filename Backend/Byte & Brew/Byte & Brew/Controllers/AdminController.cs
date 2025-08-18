using Byte___Brew.Data;
using Byte___Brew.Dtos.Admin;
using Byte___Brew.Dtos.NewFolder;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byte___Brew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public AdminsController(ByteAndBrewDbContext db) => _db = db;

        [HttpGet]
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
        public async Task<IActionResult> Create(AdminCreateDto dto)
        {
            var existingAdmin = await _db.Admins
                .FirstOrDefaultAsync(a => a.Username == dto.Username);
            if (existingAdmin != null) return BadRequest("Admin with this username already exists.");

            var admin = new Admin
            {
                Username = dto.Username,
                PasswordHash = dto.PasswordHash
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
        public async Task<IActionResult> Update(int id, AdminCreateDto dto)
        {
            var admin = await _db.Admins.FindAsync(id);
            if (admin == null) return NotFound($"There's no admin with id {id}");

            admin.Username = dto.Username;
            admin.PasswordHash = dto.PasswordHash;

            await _db.SaveChangesAsync();

            // return updated entity as DTO
            var readDto = new AdminReadDto
            {
                Id = admin.Id,
                Username = admin.Username
            };

            return Ok(readDto);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Admins.FindAsync(id);
            if (t == null) return NotFound($"There's no admin with id {id}");
            _db.Admins.Remove(t);
            await _db.SaveChangesAsync();
            return Ok($"The admin with id {id} has been deleted");
        }
    }
}
