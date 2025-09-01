using System.ComponentModel.DataAnnotations;

namespace Byte___Brew.Dtos.MenuItem
{
    public class MenuItemCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = default!;

        [Required, Range(0.1, 9999.99)]
        public decimal Price { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = default!;

        public bool IsPopular { get; set; }

        [Url]
        public string? ImageUrl { get; set; }
    }
}