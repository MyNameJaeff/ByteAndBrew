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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var tables = await _db.Tables
                .Include(t => t.Bookings)
                .ToListAsync();

            var dtoList = tables.Select(t => new TableReadDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Bookings = t.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    CustomerId = b.CustomerId,
                    TableId = b.TableId
                }).ToList()
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            var table = await _db.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return NotFound($"The table with id {id} does not exist");

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
                    CustomerId = b.CustomerId,
                    TableId = b.TableId
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("available/{date}/{time}/{people}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableTables(DateTime date, TimeSpan time, int people)
        {
            var startTime = date.Date.Add(time);
            var endTime = startTime.AddHours(2);
            var availableTables = await _db.Tables
                .Where(t => t.Capacity >= people &&
                            !_db.Bookings.Any(b => b.TableId == t.Id &&
                                                   b.StartTime < endTime &&
                                                   b.StartTime >= startTime))
                .ToListAsync();
            if (!availableTables.Any())
                return NotFound("No available tables found for the specified criteria.");
            var dtoList = availableTables.Select(t => new TableReadDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Bookings = new List<BookingReadDto>()
            }).ToList();
            return Ok(dtoList);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(TableCreateDto dto)
        {
            var existing = await _db.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == dto.TableNumber);

            if (existing != null)
                return BadRequest($"Table number {dto.TableNumber} already exists.");

            var newTable = new Table
            {
                TableNumber = dto.TableNumber,
                Capacity = dto.Capacity
            };

            _db.Tables.Add(newTable);
            await _db.SaveChangesAsync();

            var readDto = new TableReadDto
            {
                Id = newTable.Id,
                TableNumber = newTable.TableNumber,
                Capacity = newTable.Capacity,
                Bookings = new()
            };

            return CreatedAtAction(nameof(Get), new { id = newTable.Id }, readDto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, TableCreateDto dto)
        {
            var table = await _db.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return NotFound($"The table with id {id} does not exist");

            table.TableNumber = dto.TableNumber;
            table.Capacity = dto.Capacity;

            await _db.SaveChangesAsync();

            var readDto = new TableReadDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                Bookings = table.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    CustomerId = b.CustomerId,
                    TableId = b.TableId
                }).ToList()
            };

            return Ok(new { Message = $"The table with id {id} has been updated.", UpdatedTable = readDto });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _db.Tables.FindAsync(id);
            if (table == null) return NotFound($"The table with id {id} does not exist");

            _db.Tables.Remove(table);
            await _db.SaveChangesAsync();

            return Ok(new { Message = $"The table with id {id} has been deleted." });
        }
    }
}
