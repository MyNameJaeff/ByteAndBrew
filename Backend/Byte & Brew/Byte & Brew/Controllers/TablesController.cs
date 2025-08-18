using Byte___Brew.Data;
using Byte___Brew.Dtos.Booking;
using Byte___Brew.Dtos.Table;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byte___Brew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public TablesController(ByteAndBrewDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Tables.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var table = await _db.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return NotFound();

            var dto = new TableReadDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                Bookings = table.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    CustomerId = b.CustomerId
                }).ToList()
            };

            return Ok(dto);
        }


        [HttpPost]
        public async Task<IActionResult> Create(TableCreateDto dto)
        {
            var table = await _db.Tables.FirstOrDefaultAsync(t => t.TableNumber == dto.TableNumber);
            if (table != null) return BadRequest("Table already exists.");

            var newTable = new Table
            {
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity
            };

            _db.Tables.Add(newTable);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newTable.Id },
                new TableReadDto
                {
                    Id = newTable.Id,
                    TableNumber = newTable.TableNumber,
                    Capacity = newTable.Capacity
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Table table)
        {
            if (id != table.Id) return BadRequest();
            _db.Entry(table).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Tables.FindAsync(id);
            if (t == null) return NotFound();
            _db.Tables.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
