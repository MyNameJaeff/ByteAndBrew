using System.ComponentModel.DataAnnotations;

namespace Byte___Brew.Dtos.Booking
{
    public class BookingCreateDto
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Range(1, 12, ErrorMessage = "Guests must be between 1 and 12")]
        public int NumberOfGuests { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public int CustomerId { get; set; }
    }
}
