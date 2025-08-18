using Byte___Brew.Dtos.Booking;

namespace Byte___Brew.Dtos.Table
{
    public class TableReadDto
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public List<BookingReadDto> Bookings { get; set; } = new List<BookingReadDto>();
    }
}
