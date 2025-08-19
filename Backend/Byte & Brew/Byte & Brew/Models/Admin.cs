using System.ComponentModel.DataAnnotations;

namespace Byte___Brew.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; } = default!;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = default!;
    }
}