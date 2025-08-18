using Byte___Brew.Data;
using Byte___Brew.Dtos.Booking;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byte___Brew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public BookingsController(ByteAndBrewDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _db.Bookings.ToListAsync();

            var dtoList = bookings.Select(b => new BookingReadDto
            {
                Id = b.Id,
                StartTime = b.StartTime,
                NumberOfGuests = b.NumberOfGuests,
                TableId = b.TableId,
                CustomerId = b.CustomerId
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);

            if (booking == null) return NotFound();

            var dto = new BookingReadDto
            {
                Id = booking.Id,
                StartTime = booking.StartTime,
                NumberOfGuests = booking.NumberOfGuests,
                TableId = booking.TableId,
                CustomerId = booking.CustomerId
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateDto dto)
        {
            var customer = await _db.Customers.FindAsync(dto.CustomerId);
            if (customer == null) return BadRequest("Customer not found.");

            var table = await _db.Tables.FindAsync(dto.TableId);
            if (table == null) return BadRequest("Table not found.");
            if (dto.NumberOfGuests > table.Capacity)
                return BadRequest("Too many guests for this table.");

            var twoHoursBefore = dto.StartTime.AddHours(-2);
            var twoHoursAfter = dto.StartTime.AddHours(2);

            bool overlap = await _db.Bookings
                .AnyAsync(b => b.TableId == dto.TableId &&
                               b.StartTime >= twoHoursBefore &&
                               b.StartTime <= twoHoursAfter);

            if (overlap) return BadRequest("Table not available.");

            var booking = new Booking
            {
                StartTime = dto.StartTime,
                NumberOfGuests = dto.NumberOfGuests,
                TableId = dto.TableId,
                CustomerId = dto.CustomerId
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            var readDto = new BookingReadDto
            {
                Id = booking.Id,
                StartTime = booking.StartTime,
                NumberOfGuests = booking.NumberOfGuests,
                TableId = booking.TableId,
                CustomerId = booking.CustomerId
            };

            return CreatedAtAction(nameof(Get), new { id = booking.Id }, readDto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Booking booking)
        {
            if (id != booking.Id) return BadRequest();
            _db.Entry(booking).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Bookings.FindAsync(id);
            if (t == null) return NotFound();
            _db.Bookings.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
