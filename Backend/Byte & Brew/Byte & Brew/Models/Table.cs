using System.ComponentModel.DataAnnotations;

namespace Byte___Brew.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required, Range(1, 100, ErrorMessage = "Table number must be greater than 0")]
        public int TableNumber { get; set; }

        [Range(1, 12, ErrorMessage = "Capacity must be at least 1 and reasonable")]
        public int Capacity { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}