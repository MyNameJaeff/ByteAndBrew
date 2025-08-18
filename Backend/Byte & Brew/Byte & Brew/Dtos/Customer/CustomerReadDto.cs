using Byte___Brew.Dtos.Booking;

namespace Byte___Brew.Dtos.Customer
{
    public class CustomerReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public List<BookingReadDto> Bookings { get; set; } = new();
    }
}
