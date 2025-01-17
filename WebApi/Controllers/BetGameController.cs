using Application.Dtos.Out;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BabyBetBack.Controllers;

[ApiController]
[Route("api/betGame")]
[Authorize]
public class BetGameController(IBetGameService betGameService) : ControllerBase
{
    
    [HttpGet]
    [Route("id/{betGameId:guid}")]
    public async Task<ActionResult<BetGameDto>> FindByIdAsync(Guid betGameId)
    {
        var bet = await betGameService.FindByIdAsync(betGameId);
        
        return Ok(bet);
    }
}