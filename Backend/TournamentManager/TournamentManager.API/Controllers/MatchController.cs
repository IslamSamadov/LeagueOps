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
    public class MatchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MatchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate/{tournamentId}")]
        [Authorize]
        public async Task<IActionResult> GenerateRoundOne(int tournamentId)
        {
            var useStringId = User.FindFirstValue("UserId");
            if (!int.TryParse(useStringId, out var userId)) return Unauthorized();

            var tournament = await _context.Tournaments
                .Include(t => t.Teams)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null) return NotFound(new { Error = $"Tournament with {tournamentId} was not found." });

            if (tournament.OrganizerId != userId)
            {
                return StatusCode(403, new { Error = "Only the organizer can generate the bracket." });
            }

            if (tournament.Matches != null && tournament.Matches.Any())
            {
                return BadRequest(new { Error = "Bracket for this tournament has already been created." });
            }

            if (tournament.Teams == null || tournament.Teams.Count < 2)
            {
                return BadRequest(new { Error = "To generate bracket you need at least 2 teams." });
            }

            //Shuffle and pair teams
            var shuffledTeams = tournament.Teams.OrderBy(t => Guid.NewGuid()).ToList();
            var roundOneMatches = new List<Match>();

            for (int i = 0; i < shuffledTeams.Count; i += 2)
            {
                var match = new Match
                {
                    TournamentId = tournament.Id,
                    RoundNumber = 1,
                    TeamAId = shuffledTeams[i].Id,

                    // If there is an odd number of teams, the last team doesn't have an opponent
                    // They get a a free win to Round 2. We leave TeamBId as null.
                    TeamBId = (i + 1 < shuffledTeams.Count) ? shuffledTeams[i + 1].Id : null
                };
                roundOneMatches.Add(match);
            }

            tournament.Status = "InProgress";
            _context.Matches.AddRange(roundOneMatches);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = $"Tournament '{tournament.Name}' has officially started! Generated {roundOneMatches.Count} matches for Round 1.",
                TotalTeams = shuffledTeams.Count
            });
        }
        [HttpPost("{match}/resolve")]
        [Authorize]
        public async Task<IActionResult> ResolveMatch(int matchId, [FromBody] MatchResolveDto request)
        {
            var userStringId = User.FindFirstValue("UserId");
            if (!int.TryParse(userStringId, out var userId)) return Unauthorized();

            var match = await _context.Matches.
                Include(m => m.Tournament)
                    .ThenInclude(t => t.Matches)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null) return NotFound(new { Error = $"Match not found." });

            if (match.Tournament!.OrganizerId != userId) return StatusCode(403, new { Error = "Onlt organizer can set match winners." });

            if (request.winnerId != match.TeamAId && request.winnerId != match.TeamBId) return BadRequest(new { Error = "The winner must be from the teams in this match." });

            match.WinnerTeamId = matchId;
            await _context.SaveChangesAsync();

            //Automatic next round logic

            var currentRoundMatches = match.Tournament!.Matches!
                .Where(m => m.RoundNumber == match.RoundNumber)
                .ToList();

            bool isRoundFinished = currentRoundMatches.Any(m => m.WinnerTeamId != null);

            if (isRoundFinished)
            {
                var advancingTeams = currentRoundMatches.Select(m => m.WinnerTeamId!.Value).ToList();

                //If its finale declare winner

                if (advancingTeams.Count == 1)
                {
                    var winnerTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Id == request.winnerId);
                    match.Tournament.Status = "Completed";
                    await _context.SaveChangesAsync();
                    return Ok(new { Message = $"Match resolved! Team {winnerTeam!.Name} has won the Tournament!" });
                }

                //If its not finale generate the round automatically

                var nextRoundMatches = new List<Match>();
                int nextRoundNumber = match.RoundNumber + 1;

                for (int i = 0; i < advancingTeams.Count; i += 2)
                {
                    var nextMatch = new Match
                    {
                        TournamentId = match.TournamentId,
                        TeamAId = advancingTeams[i],
                        RoundNumber = nextRoundNumber,
                        //Handling odd number of teams
                        TeamBId = (i + 1 < advancingTeams.Count) ? advancingTeams[i + 1] : null
                    };

                    nextRoundMatches.Add(nextMatch);
                }

                _context.Matches.AddRange(nextRoundMatches);
                await _context.SaveChangesAsync();

                return Ok(new {Message = $"Match resolved! Round {match.RoundNumber} is finished. Round {nextRoundNumber} has benn generated."});
            }

            return Ok(new { Message = "Match resolved! Waiting for other matches in this round to finish." });
        }

    }
}
