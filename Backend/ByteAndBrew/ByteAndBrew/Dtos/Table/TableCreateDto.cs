using System.ComponentModel.DataAnnotations;

namespace ByteAndBrew.Dtos.Table
{
    public class TableCreateDto
    {
        [Required, Range(1, 200, ErrorMessage = "Table number must be between 1 and 200")]
        public int TableNumber { get; set; }

        [Required, Range(1, 12, ErrorMessage = "Capacity must be between 1 and 12")]
        public int Capacity { get; set; }
    }
}
