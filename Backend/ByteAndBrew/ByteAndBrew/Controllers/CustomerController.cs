using ByteAndBrew.Data;
using ByteAndBrew.Dtos.Booking;
using ByteAndBrew.Dtos.Customer;
using ByteAndBrew.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ByteAndBrew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public CustomersController(ByteAndBrewDbContext db) => _db = db;

        [Authorize] // Only admins should get the customers info to stop others from accesing them
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _db.Customers
                .Include(c => c.Bookings)
                .ToListAsync();

            var dtoList = customers.Select(c => new CustomerReadDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                Bookings = c.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId,
                    CustomerId = b.CustomerId
                }).ToList()
            }).ToList();

            return Ok(dtoList);
        }

        [Authorize] // Only admins should get the customers info to stop others from accesing them
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var customer = await _db.Customers
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return NotFound($"The customer with id {id} does not exist");

            var dto = new CustomerReadDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Bookings = customer.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId,
                    CustomerId = b.CustomerId
                }).ToList()
            };

            return Ok(dto);
        }

        [Authorize] // Only admins should get the customers info to stop others from accesing them
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomer([FromQuery] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Phone number is required");

            var customer = await _db.Customers
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber.Trim());

            if (customer == null)
                return Ok(null); // return null if not found

            var dto = new CustomerReadDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Bookings = customer.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId,
                    CustomerId = b.CustomerId
                }).ToList()
            };

            return Ok(dto);
        }


        [AllowAnonymous] // Thinking if the users are allowed to create a customer (might change depending on)
        [HttpPost]
        public async Task<IActionResult> Create(CustomerCreateDto dto)
        {
            var existingCustomer = await _db.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == dto.PhoneNumber);

            if (existingCustomer != null)
                return BadRequest("Customer with this phone number already exists.");

            var customer = new Customer
            {
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            var readDto = new CustomerReadDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Bookings = new List<BookingReadDto>()
            };

            return CreatedAtAction(nameof(Get), new { id = customer.Id }, readDto);
        }

        [Authorize] // Should customers be allowed to change their information? (no login means others could change theirs :/
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CustomerCreateDto dto)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return NotFound($"The customer with id {id} does not exist");

            customer.Name = dto.Name;
            customer.PhoneNumber = dto.PhoneNumber;

            await _db.SaveChangesAsync();

            var readDto = new CustomerReadDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Bookings = customer.Bookings.Select(b => new BookingReadDto
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    NumberOfGuests = b.NumberOfGuests,
                    TableId = b.TableId,
                    CustomerId = b.CustomerId
                }).ToList()
            };

            return Ok(readDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return NotFound($"The customer with id {id} does not exist");

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();

            return Ok($"The customer with id {id} has been removed");
        }
    }
}
