using Microsoft.EntityFrameworkCore;
using TournamentManager.API.Entities;

namespace TournamentManager.API.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //Relationship 1: Team A
            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamA)
                .WithMany()
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            //Realtionship 2: Team B
            modelBuilder.Entity<Match>()
                .HasOne(m => m.TeamB)
                .WithMany()
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            //Realtionship 3: Winner Team
            modelBuilder.Entity<Match>()
                .HasOne(m => m.WinnerTeam)
                .WithMany()
                .HasForeignKey(m => m.WinnerTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship 4: Next Match (Self-Referencing)
            // When this match finishes, the winner goes to 'NextMatch'
            modelBuilder.Entity<Match>()
                .HasOne(m => m.NextMatch)
                .WithMany()
                .HasForeignKey(m => m.NextMatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
