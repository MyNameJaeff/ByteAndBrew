using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ByteAndBrew.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Range(1, 12, ErrorMessage = "Number of guests must be between 1 and 12, call us if more")]
        public int NumberOfGuests { get; set; }

        [Required]
        [ForeignKey(nameof(Table))]
        public int TableId { get; set; }
        public Table Table { get; set; } = default!;

        [Required]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
    }

}
