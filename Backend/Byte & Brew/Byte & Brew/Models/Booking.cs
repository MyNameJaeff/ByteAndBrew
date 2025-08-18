namespace Byte___Brew.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int NumberOfGuests { get; set; }

        // Foreign Keys
        public int TableId { get; set; }
        public Table Table { get; set; } = default!;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
    }
}
