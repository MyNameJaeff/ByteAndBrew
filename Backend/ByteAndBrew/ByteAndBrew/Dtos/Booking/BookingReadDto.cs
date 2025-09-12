namespace ByteAndBrew.Dtos.Booking
{
    public class BookingReadDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int NumberOfGuests { get; set; }
        public int TableId { get; set; }
        public int CustomerId { get; set; }
    }

    public class BookingReadDetailedDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int NumberOfGuests { get; set; }
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public int TableCapacity { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public string CustomerPhoneNumber { get; set; } = default!;
    }
}
