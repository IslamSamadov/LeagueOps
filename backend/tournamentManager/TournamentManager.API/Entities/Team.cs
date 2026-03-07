namespace TournamentManager.API.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public List<Player>? Players { get; set; }
    }
}
