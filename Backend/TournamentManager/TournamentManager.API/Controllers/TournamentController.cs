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
                .Select(t => new TournamentResponseDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Game = t.Game,
                    StartDate = t.StartDate,
                    MaxTeams = t.MaxTeams,
                    Status = t.Status,
                    OrganizerName = t.Organizer != null ? t.Organizer.Username : "Unknown"
                })
                .ToListAsync();

            return Ok(tournaments);
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
    }
}