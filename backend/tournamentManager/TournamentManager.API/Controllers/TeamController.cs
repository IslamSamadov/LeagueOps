using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class TeamController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeamController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTeam(TeamCreateDto request)
        {
            // Find out exactly who is making this request
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId);

            if(tournament == null)
            {
                return NotFound(new {Error = $"Tournament with ID {request.TournamentId} was not found."});
            }

            // Checking if it is tournament organizer 
            if (tournament.OrganizerId != userId)
            {
                return StatusCode(403, new { Error = "Only the tournament organizer can delete teams." });
            }

            int currentTeamCount = tournament.Teams?.Count ?? 0;

            if(currentTeamCount >= tournament.MaxTeams)
            {
                return BadRequest(new {Error = $"Registration closed! The tournament {tournament.Name} is already full {tournament.MaxTeams}/{tournament.MaxTeams}."});
            }

            var newTeam = new Team
            {
                Name = request.Name,
                TournamentId = request.TournamentId
            };

            _context.Teams.Add(newTeam);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = $"Team '{newTeam.Name}' has been registered for '{tournament.Name}' succesfully.",
                CurrentCapacity = $"{currentTeamCount + 1}/{tournament.MaxTeams}"
            });
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            // Find out exactly who is making this request
            var userIdString = User.FindFirstValue("UserId");
            if(!int.TryParse(userIdString, out var userId)) return Unauthorized();

            var team = await _context.Teams
                    .Include(t => t.Tournament) 
                    .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                return NotFound(new {Error = $"Team with ID {id} was not found."});
            }

            // Checking if it is tournament organizer 
            if(team.Tournament!.OrganizerId != userId)
            {
                return StatusCode(403, new { Error = "Only the tournament organizer can delete teams." });
            }
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = $"Team '{team.Name}' has been deleted succesfully"
            });
        }
    }
}
