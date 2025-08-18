namespace Byte___Brew.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        public List<Booking> Bookings { get; set; } = new();
    }
}
