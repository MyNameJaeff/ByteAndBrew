namespace Byte___Brew.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
    }
}
