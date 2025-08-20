namespace Byte___Brew.Dtos.Menu
{
    public class MenuItemReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsPopular { get; set; }
        public string? ImageUrl { get; set; }
    }
}
