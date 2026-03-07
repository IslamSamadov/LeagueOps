namespace TournamentManager.API.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? InGameId { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
    }
}
