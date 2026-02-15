using System.ComponentModel.DataAnnotations;
using TournamentManager.API.Validations;

namespace TournamentManager.API.DTOs
{
    public class TournamentCreateDto
    {
        [Required(ErrorMessage = "Please provide a tournament name.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please specify the game.")]
        [MaxLength(50)]
        public string Game {  get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        [AllowedTeamCount] // Custom Validation
        public int MaxTeams { get; set; }
    }
}
