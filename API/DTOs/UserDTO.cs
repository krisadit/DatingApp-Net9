using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserDTO
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        public required string KnownAs { get; set; }

        public string? PhotoUrl { get; set; }

        
    }
}
