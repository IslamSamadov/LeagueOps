namespace TournamentManager.API.DTOs
{
    public class TournamentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int MaxTeams { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OrganizerName { get; set; } = string.Empty;
    }
}
