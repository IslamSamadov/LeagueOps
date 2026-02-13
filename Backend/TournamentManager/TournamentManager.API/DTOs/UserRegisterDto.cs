using System.ComponentModel.DataAnnotations;

namespace TournamentManager.API.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        [MinLength(8)]
        public string? Password { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
