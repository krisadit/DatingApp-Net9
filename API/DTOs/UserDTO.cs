using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
