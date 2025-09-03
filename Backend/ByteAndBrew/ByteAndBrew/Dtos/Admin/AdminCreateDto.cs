using System.ComponentModel.DataAnnotations;

namespace ByteAndBrew.Dtos.Admin
{
    public class AdminCreateDto
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = default!;

        [Required, StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;
    }
}
