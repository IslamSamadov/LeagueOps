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
    public class PlayerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlayerController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPlayerToTeam(PlayerCreateDto request)
        {
            var userIdString = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

            var team = await _context.Teams
                .Include(t => t.Tournament)
                .FirstOrDefaultAsync(t => t.Id == request.TeamId);

            if (team == null) return NotFound(new { Error = $"Team with ID {request.TeamId} was not found." });

            if(team.Tournament!.OrganizerId != userId)
            {
                return StatusCode(403, new { Error = "Only the tournament organizer can add players to teams." });
            }

            var newPlayer = new Player
            {
                Name = request.Name,
                InGameId = request.InGameId,
                TeamId = team.Id,
            };

            _context.Players.Add(newPlayer);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Player '{newPlayer.Name}' successfully added to team '{team.Name}'!",
                PlayerId = newPlayer.Id
            });
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var userStringId = User.FindFirstValue("UserId");
            if(!int.TryParse(userStringId, out var userId)) return Unauthorized();

            var player = await _context.Players
                .Include(p => p.Team)
                    .ThenInclude(t => t.Tournament)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return NotFound(new {Error = $"Player with {id} id was not found."});

            if(player.Team!.Tournament!.OrganizerId != userId)
            {
                return StatusCode(403, new { Error = "Only the tournament organizer can delete players." });
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Player '{player.Name}' has been deleted succesfully"
            });

        }
    }
}
