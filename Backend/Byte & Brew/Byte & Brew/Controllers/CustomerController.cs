using Byte___Brew.Data;
using Byte___Brew.Dtos.Booking;
using Byte___Brew.Dtos.Customer;
using Byte___Brew.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Byte___Brew.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ByteAndBrewDbContext _db;
        public CustomersController(ByteAndBrewDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Customers.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var customer = await _db.Customers
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return NotFound();

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

        [HttpPost]
        public async Task<IActionResult> Create(CustomerCreateDto dto)
        {
            // Optional: check if a customer with the same phone number already exists
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

            // Optionally, return a DTO instead of the entity
            return CreatedAtAction(nameof(Get), new { id = customer.Id },
                new CustomerReadDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    PhoneNumber = customer.PhoneNumber
                });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Customer customer)
        {
            if (id != customer.Id) return BadRequest();
            _db.Entry(customer).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var t = await _db.Customers.FindAsync(id);
            if (t == null) return NotFound();
            _db.Customers.Remove(t);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
