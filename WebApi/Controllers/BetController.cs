using Application.Dtos;
using Application.Dtos.In;
using Application.Dtos.Out;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace BabyBetBack.Controllers;

[ApiController]
[Route("api/bet")]
public class BetController(IBetService betService) : ControllerBase
{
    [HttpPost]
    [Route("add")]
    public async Task<ActionResult<BetDto>> AddAsync(CreateUserBetRequest request)
    {
        var bet = await betService.CreateBetAsync(request, User.GetNameIdentifierId());

        return Created();
        //return CreatedAtAction(nameof(FindById), new { bet = bet });
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
    
    
    // TODO : Delete Before prod
    [HttpPost]
    [Route("delete/{betId:guid}")]
    public async Task<ActionResult> DeleteById(Guid betId)
    {
        await betService.DeleteAsync(betId);
        
        return Ok();
    }
    
    
    [HttpGet]
    [Route("test")]
    public IActionResult Test()
    {
        try
        {
            var betGame = betService.GetUser();
            return Ok(betGame);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(e.Message);
        }
    }
}