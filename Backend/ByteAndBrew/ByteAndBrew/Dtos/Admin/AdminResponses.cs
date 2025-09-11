using System.ComponentModel.DataAnnotations;

namespace ByteAndBrew.Dtos.Admin
{
    public class AdminResponses
    {
        public string Token { get; set; } = string.Empty;
        public int AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class AdminLoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }


    public class AdminCreateDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;
    }

    public class AdminUpdateDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
        public string Username { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string? Password { get; set; }
    }

    public class AdminReadDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
    }

}