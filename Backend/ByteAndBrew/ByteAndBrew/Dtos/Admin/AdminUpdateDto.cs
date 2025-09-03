using System.ComponentModel.DataAnnotations;

namespace ByteAndBrew.Dtos.Admin
{
    public class AdminUpdateDto
    {
        [Required, StringLength(50)]
        public string Username { get; set; } = default!;

        [StringLength(255)]
        [DataType(DataType.Password)]
        public string? Password { get; set; } // Optional for updates
    }
}