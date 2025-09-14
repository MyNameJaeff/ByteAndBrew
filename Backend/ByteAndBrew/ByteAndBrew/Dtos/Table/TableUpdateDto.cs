using System.ComponentModel.DataAnnotations;

namespace ByteAndBrew.Dtos.Table
{
    public class TableUpdateDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Table number must be positive")]
        public int TableNumber { get; set; }

        [Required, Range(1, 12, ErrorMessage = "Capacity must be between 1 and 12")]
        public int Capacity { get; set; }
    }
}