using System.ComponentModel.DataAnnotations;

namespace TournamentManager.API.DTOs
{
    public class PlayerCreateDto
    {
        [Required]
        public string? Name { get; set; }
        public string? InGameId { get; set; }
        public int TeamId { get; set; }
    }
}
