namespace TournamentManager.API.Entities
{
    public class Match
    {
        public int Id { get; set; }
        public int RoundNumber { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int? TeamAId { get; set; }
        public Team? TeamA { get; set; }
        public int? TeamBId { get; set; }
        public Team? TeamB { get; set; }
        public int? WinnerTeamId { get; set; }
        public Team? WinnerTeam { get; set; }

        public int? NextMatchId { get; set; }
        public Match? NextMatch { get; set; }
    }
}
