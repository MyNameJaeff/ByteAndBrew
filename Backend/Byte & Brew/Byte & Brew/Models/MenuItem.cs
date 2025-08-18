namespace Byte___Brew.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public string Description { get; set; } = default!;
        public bool IsPopular { get; set; }
        public string? ImageUrl { get; set; }
    }
}
