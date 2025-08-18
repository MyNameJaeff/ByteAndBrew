namespace Byte___Brew.Dtos.Booking
{
    public class BookingCreateDto
    {
        public DateTime StartTime { get; set; }
        public int NumberOfGuests { get; set; }
        public int TableId { get; set; }
        public int CustomerId { get; set; }
    }
}
