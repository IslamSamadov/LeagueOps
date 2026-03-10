namespace TournamentManager.API.DTOs
{
    public class MatchResponseDto
    {
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public int? TeamAId { get; set; }
        public string TeamAName { get; set; } = "TBD";
        public int? TeamBId { get; set; }
        public string TeamBName { get; set; } = "TBD";
        public int? WinnerTeamId { get; set; }
        public string? WinnerName { get; set; }
        public int? NextMatchId { get; set; }
    }
}
