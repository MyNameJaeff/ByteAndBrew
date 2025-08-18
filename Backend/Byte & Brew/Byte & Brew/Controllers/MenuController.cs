using Byte___Brew.Data;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byte___Brew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public MenuItemsController(ByteAndBrewDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.MenuItems.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) =>
            await _db.MenuItems.FindAsync(id) is MenuItem t ? Ok(t) : NotFound();

        [HttpPost]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            _db.MenuItems.Add(menuItem);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = menuItem.Id }, menuItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MenuItem menuItem)
        {
            if (id != menuItem.Id) return BadRequest();
            _db.Entry(menuItem).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.MenuItems.FindAsync(id);
            if (t == null) return NotFound();
            _db.MenuItems.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
