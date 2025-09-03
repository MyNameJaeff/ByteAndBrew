using System.ComponentModel.DataAnnotations;

namespace ByteAndBrew.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Required, Phone, StringLength(20)]
        public string PhoneNumber { get; set; } = default!;

        public List<Booking> Bookings { get; set; } = new();
    }
}