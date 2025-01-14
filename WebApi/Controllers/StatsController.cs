using Application.Dtos.Out.Stats;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyBetBack.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize]
public class StatsController(IStatsService statsService) : ControllerBase
{
    [HttpGet]
    [Route("{betGameId:guid}")]
    public async Task<ActionResult<StatsDto>> GetStatsAsync(Guid betGameId)
    {
        var bet = await statsService.GetStatsAsync(betGameId);
        
        return Ok(bet);
    }
}