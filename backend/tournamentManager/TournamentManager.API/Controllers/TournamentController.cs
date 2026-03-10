using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TournamentManager.API.Data;
using TournamentManager.API.DTOs;
using TournamentManager.API.Entities;

namespace TournamentManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TournamentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTournaments()
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Teams)
                .Select(t => new TournamentResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Game = t.Game,
                    StartDate = t.StartDate,
                    MaxTeams = t.MaxTeams,
                    Status = t.Status,
                    TeamCount = t.Teams != null ? t.Teams.Count() : 0,
                    OrganizerName = t.Organizer != null ? t.Organizer.Username : "Unknown"
                })
                .ToListAsync();

            return Ok(tournaments);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyTournaments()
        {
            // 1. Extract the UserId from the JWT token
            var userStringId = User.FindFirstValue("UserId");
            if (!int.TryParse(userStringId, out var userId)) return Unauthorized();

            // 2. Filter the database query
            var tournaments = await _context.Tournaments
                    .Where(t => t.OrganizerId == userId)
                    .Include(t => t.Teams) // Ensure Teams are loaded for the count
                    .Select(t => new TournamentResponseDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Game = t.Game,
                        StartDate = t.StartDate,
                        MaxTeams = t.MaxTeams,
                        Status = t.Status,
                        TeamCount = t.Teams != null ? t.Teams.Count() : 0,
                        OrganizerName = t.Organizer != null ? t.Organizer.Username : "Unknown"
                    })
                    .ToListAsync();

            return Ok(tournaments);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetTournamentById(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Organizer)
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.TeamA)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.TeamB)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound($"Tournament with ID {id} was not found.");
            }

            var userStringId = User.FindFirstValue("UserId");
            bool isOrganizer = int.TryParse(userStringId, out var userId) && tournament.OrganizerId == userId;

            var response = new TournamentDetailResponseDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Game = tournament.Game,
                StartDate = tournament.StartDate,
                MaxTeams = tournament.MaxTeams,
                Status = tournament.Status,
                OrganizerId = tournament.OrganizerId,
                OrganizerName = tournament.Organizer?.Username ?? "Unknown",
                IsOrganizer = isOrganizer,
                Teams = tournament.Teams?.Select(team => new TeamResponseDto
                {
                    Id = team.Id,
                    Name = team.Name
                }).ToList() ?? new List<TeamResponseDto>(),
                Matches = tournament.Matches?.Select(m => new MatchResponseDto
                {
                    Id = m.Id,
                    RoundNumber = m.RoundNumber,
                    TeamAId = m.TeamAId,
                    TeamAName = m.TeamA?.Name ?? "TBD",
                    TeamBId = m.TeamBId,
                    TeamBName = m.TeamB?.Name ?? "TBD",
                    WinnerTeamId = m.WinnerTeamId,
                    WinnerName = m.WinnerTeam?.Name,
                    NextMatchId = m.NextMatchId
                }).OrderBy(m => m.RoundNumber).ToList() ?? new List<MatchResponseDto>()
            };

            return Ok(response);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTournament(TournamentCreateDto request)
        {
            //1. Read the JWT Token to find out WHO is making this request
            var userIdString = User.FindFirstValue("UserId");

            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            //2. Maping the DTO
            var newTournament = new Tournament
            {
                Name = request.Name,
                Game = request.Game,
                StartDate = request.StartDate,
                MaxTeams = request.MaxTeams,
                OrganizerId = userId // Sets Logged in User as organizer.
            };

            _context.Tournaments.Add(newTournament);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Tournament '{newTournament.Name}' created successfully!",
                TournamentId = newTournament.Id
            });
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTournament(int id)
        {
            var userStringId = User.FindFirstValue("UserId");
            if (!int.TryParse(userStringId, out var userId)) return Unauthorized();
            var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            if (userId != tournament.OrganizerId)
            {
                return StatusCode(403, new { Error = "Only tournament organizer can delete it." });
            }

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Tournament '{tournament.Name}' was succesfully deleted.'"
            });
        }
    }
}