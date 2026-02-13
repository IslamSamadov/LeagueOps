namespace TournamentManager.API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public List<Tournament>? Tournaments { get; set; }
    }
}
