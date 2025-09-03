using ByteAndBrew.Dtos.Booking;

namespace ByteAndBrew.Dtos.Table
{
    public class TableReadDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public List<BookingReadDto> Bookings { get; set; } = new List<BookingReadDto>();

        // A property for the normal users to check all tables
        public bool? IsBooked { get; set; }
    }
}
