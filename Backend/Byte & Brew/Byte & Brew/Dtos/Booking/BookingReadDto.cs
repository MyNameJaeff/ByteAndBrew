namespace Byte___Brew.Dtos.Booking
{
    public class BookingReadDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int NumberOfGuests { get; set; }
        public int TableId { get; set; }
        public int CustomerId { get; set; }
    }
}
