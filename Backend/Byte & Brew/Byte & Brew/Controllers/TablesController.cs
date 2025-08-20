using Byte___Brew.Data;
using Byte___Brew.Dtos.Booking;
using Byte___Brew.Dtos.Table;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var tables = await _db.Tables
                .Include(t => t.Bookings)
                .ToListAsync();

            bool isAuthorized = User?.Identity?.IsAuthenticated ?? false;

            var dtoList = tables.Select(t => new TableReadDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Bookings = isAuthorized
                    ? t.Bookings.Select(b => new BookingReadDto
                    {
                        Id = b.Id,
                        StartTime = b.StartTime,
                        NumberOfGuests = b.NumberOfGuests,
                        CustomerId = b.CustomerId,
                        TableId = b.TableId
                    }).ToList()
                    : new List<BookingReadDto>(),

                IsBooked = t.Bookings.Any()
            }).ToList();

            return Ok(dtoList);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            var table = await _db.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return NotFound($"The table with id {id} does not exist");

            bool isAuthorized = User?.Identity?.IsAuthenticated ?? false;

            var dto = new TableReadDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                Bookings = isAuthorized
                    ? table.Bookings.Select(b => new BookingReadDto
                    {
                        Id = b.Id,
                        StartTime = b.StartTime,
                        NumberOfGuests = b.NumberOfGuests,
                        CustomerId = b.CustomerId,
                        TableId = b.TableId
                    }).ToList()
                    : new List<BookingReadDto>(),
                IsBooked = table.Bookings.Any()
            };

            return Ok(dto);
        }

        [AllowAnonymous]
        [HttpGet("available")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableTables([FromQuery] DateTime date, [FromQuery] TimeSpan time, [FromQuery] int people)
        {
            if (people <= 0)
                return BadRequest("Number of people must be greater than 0.");

            var requestedStart = date.Date.Add(time);
            var requestedEnd = requestedStart.AddHours(2);

            // Find tables that have sufficient capacity and no conflicting bookings
            var availableTables = await _db.Tables
                .Where(t => t.Capacity >= people)
                .Where(t => !_db.Bookings.Any(b =>
                    b.TableId == t.Id &&
                    !(requestedEnd <= b.StartTime || requestedStart >= b.StartTime.AddHours(2))))
                .ToListAsync();

            if (!availableTables.Any())
                return NotFound("No available tables found for the specified criteria.");

            var dtoList = availableTables.Select(t => new TableReadDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Bookings = new List<BookingReadDto>(),
                IsBooked = false
            }).ToList();

            return Ok(dtoList);
        }

        [Authorize]
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
                Bookings = new List<BookingReadDto>(),
                IsBooked = false
            };

            return CreatedAtAction(nameof(Get), new { id = newTable.Id }, readDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, TableCreateDto dto)
        {
            var table = await _db.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return NotFound($"The table with id {id} does not exist");

            // Check if the new table number conflicts with existing tables
            var existing = await _db.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == dto.TableNumber && t.Id != id);

            if (existing != null)
                return BadRequest($"Table number {dto.TableNumber} already exists.");

            // Check if reducing capacity would conflict with existing bookings
            if (dto.Capacity < table.Capacity)
            {
                var futureBookings = table.Bookings
                    .Where(b => b.StartTime > DateTime.Now && b.NumberOfGuests > dto.Capacity)
                    .ToList();

                if (futureBookings.Any())
                    return BadRequest($"Cannot reduce capacity. There are future bookings with {futureBookings.Max(b => b.NumberOfGuests)} guests.");
            }

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
                }).ToList(),
                IsBooked = table.Bookings.Any()
            };

            return Ok(readDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _db.Tables
                .Include(t => t.Bookings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null) return NotFound($"The table with id {id} does not exist");

            // Check if there are future bookings for this table
            var futureBookings = table.Bookings.Where(b => b.StartTime > DateTime.Now).ToList();
            if (futureBookings.Any())
                return BadRequest("Cannot delete table with future bookings. Cancel the bookings first.");

            _db.Tables.Remove(table);
            await _db.SaveChangesAsync();

            return Ok($"The table with id {id} has been deleted");
        }
    }
}