using System.ComponentModel.DataAnnotations;

namespace TournamentManager.API.DTOs
{
    public class UserLoginDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
