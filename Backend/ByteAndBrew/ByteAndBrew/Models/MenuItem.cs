using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Byte___Brew.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Range(0.01, 1000, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(10,2)")] // ensures correct precision in SQL
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = default!;

        public bool IsPopular { get; set; }

        [Url, StringLength(300)]
        public string? ImageUrl { get; set; }
    }
}