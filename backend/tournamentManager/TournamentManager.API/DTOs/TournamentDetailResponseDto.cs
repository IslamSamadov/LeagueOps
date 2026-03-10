namespace TournamentManager.API.DTOs
{
    public class TournamentDetailResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Game { get; set; }
        public DateTime StartDate {  get; set; }
        public int MaxTeams { get; set; }
        public string? Status { get; set; }
        public int OrganizerId { get; set; }
        public string? OrganizerName { get; set; }
        public bool IsOrganizer { get; set; }
        public List<TeamResponseDto>? Teams { get; set; }
        public List<MatchResponseDto> Matches { get; set; } = new();
    }
}
