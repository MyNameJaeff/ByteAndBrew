using ByteAndBrew.Dtos.Booking;

namespace ByteAndBrew.Dtos.Customer
{
    public class CustomerReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public List<BookingReadDto> Bookings { get; set; } = new();
    }
}
