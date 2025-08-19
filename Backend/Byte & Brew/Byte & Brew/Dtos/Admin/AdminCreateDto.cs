using System.ComponentModel.DataAnnotations;

namespace Byte___Brew.Dtos.NewFolder
{
    public class AdminCreateDto
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = default!;

        [Required, StringLength(255)]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; } = default!;
    }
}
