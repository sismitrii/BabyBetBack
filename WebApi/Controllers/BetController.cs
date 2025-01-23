using Application.Dtos.In;
using Application.Dtos.Out;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace BabyBetBack.Controllers;

[ApiController]
[Route("api/bet")]
[Authorize]
public class BetController(IBetService betService) : ControllerBase
{
    [HttpPost]
    [Route("add")]
    public async Task<ActionResult<BetDto>> AddAsync(CreateUserBetRequest request)
    {
        var bet = await betService.CreateBetAsync(request, User.GetNameIdentifierId());

        return Ok(bet); // TODO : find a solution to return a Created with the bet inside
    }

    
    [HttpGet]
    [Route("id/{betId:guid}")]
    public async Task<ActionResult<BetDto>> FindByIdAsync(Guid betId)
    {
        var bet = await betService.FindByIdAsync(betId);
        
        return Ok(bet);
    }
   
    [HttpGet]
    [Route("user-logged")]
    public async Task<ActionResult<BetDto>> FindBetOfUserAsync()
    {
        var bet = await betService.FindBetOfUserAsync(User.GetNameIdentifierId());
        
        return Ok(bet);
    }

    [HttpGet]
    [Route("betGame/{betGameId:guid}")]
    public async Task<ActionResult<BetDto>> GetAllAsync(Guid betGameId)
    {
        var bets = await betService.GetAllForAGameAsync(betGameId);
        
        return Ok(bets);
    }
    
    
    [HttpDelete]
    [Route("delete/{betId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteById(Guid betId)
    {
        await betService.DeleteAsync(betId);
        
        return Ok();
    }
    
    [HttpPut]
    [Route("update/{betId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateAsync(Guid betId, UpdateUserBetRequest request)
    {
        await betService.UpdateAsync(betId, request);
        
        return Ok();
    }
}