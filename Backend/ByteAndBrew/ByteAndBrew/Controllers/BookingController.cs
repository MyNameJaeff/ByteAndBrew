using ByteAndBrew.Data;
using ByteAndBrew.Dtos.Booking;
using ByteAndBrew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteAndBrew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public BookingsController(ByteAndBrewDbContext db) => _db = db;

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);

            if (booking == null) return NotFound($"The booking with id {id} does not exist");

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

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(BookingCreateDto dto)
        {
            // Validate customer exists
            var customer = await _db.Customers.FindAsync(dto.CustomerId);
            if (customer == null) return BadRequest("Customer not found.");

            // Validate table exists and capacity
            var table = await _db.Tables.FindAsync(dto.TableId);
            if (table == null) return BadRequest("Table not found.");
            if (dto.NumberOfGuests > table.Capacity)
                return BadRequest("Too many guests for this table.");
            if (dto.NumberOfGuests <= 0)
                return BadRequest("Number of guests must be greater than 0.");

            // Check for booking conflicts using correct overlap logic
            if (await HasBookingConflict(dto.TableId, dto.StartTime, null))
            {
                return BadRequest("Table is not available at the requested time. Remember that each booking reserves the table for 2 hours.");
            }

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

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(int id, BookingCreateDto dto)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null) return NotFound($"There's no booking with id {id}");

            // Validate customer exists
            var customer = await _db.Customers.FindAsync(dto.CustomerId);
            if (customer == null) return BadRequest("Customer not found.");

            // Validate table exists and capacity
            var table = await _db.Tables.FindAsync(dto.TableId);
            if (table == null) return BadRequest("Table not found.");
            if (dto.NumberOfGuests > table.Capacity)
                return BadRequest("Too many guests for this table.");
            if (dto.NumberOfGuests <= 0)
                return BadRequest("Number of guests must be greater than 0.");

            // Check for conflicts, excluding the current booking
            if (await HasBookingConflict(dto.TableId, dto.StartTime, id))
            {
                return BadRequest("Table is not available at the requested time. Remember that each booking reserves the table for 2 hours.");
            }

            booking.TableId = dto.TableId;
            booking.CustomerId = dto.CustomerId;
            booking.StartTime = dto.StartTime;
            booking.NumberOfGuests = dto.NumberOfGuests;

            await _db.SaveChangesAsync();

            var readDto = new BookingReadDto
            {
                Id = booking.Id,
                StartTime = booking.StartTime,
                NumberOfGuests = booking.NumberOfGuests,
                TableId = booking.TableId,
                CustomerId = booking.CustomerId
            };

            return Ok(readDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null) return NotFound($"The booking with id {id} does not exist");

            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();

            return Ok($"The booking with id {id} has been removed");
        }

        /// <summary>
        /// Checks if a booking conflicts with existing bookings for the same table.
        /// A booking reserves a table for 2 hours, so conflicts occur when time periods overlap.
        /// </summary>
        /// <param name="tableId">The table to check</param>
        /// <param name="startTime">The start time of the new/updated booking</param>
        /// <param name="excludeBookingId">Booking ID to exclude from conflict check (for updates)</param>
        /// <returns>True if there is a conflict, false otherwise</returns>
        private async Task<bool> HasBookingConflict(int tableId, DateTime startTime, int? excludeBookingId)
        {
            var requestedEndTime = startTime.AddHours(2);

            var conflictingBooking = await _db.Bookings
                .Where(b => b.TableId == tableId)
                .Where(b => excludeBookingId == null || b.Id != excludeBookingId)
                .Where(b => !(requestedEndTime <= b.StartTime || startTime >= b.StartTime.AddHours(2)))
                .FirstOrDefaultAsync();

            return conflictingBooking != null;
        }

        [HttpGet("available-times")]
        public async Task<IActionResult> GetAvailableTimes(int tableId, DateTime date)
        {
            // Get all bookings for this table on the given day
            var bookings = await _db.Bookings
                .Where(b => b.TableId == tableId && b.StartTime.Date == date.Date)
                .Select(b => b.StartTime)
                .ToListAsync();

            return Ok(bookings); // return booked start times
        }

    }
}