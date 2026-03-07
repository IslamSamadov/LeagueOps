namespace TournamentManager.API.Entities
{
    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public int MaxTeams { get; set; }
        public string Status { get; set; } = "Draft";


        public int OrganizerId { get; set; }
        public User? Organizer { get; set; }


        public List<Team>? Teams { get; set; }
        public List<Match>? Matches { get; set; }
    }
}
