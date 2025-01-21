using Application.Configuration;
using Application.Dtos.In;
using Application.Exceptions;
using Application.Services.Auth;
using Application.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace BabyBetBack.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IOptions<GoogleAuthConfig> googleAuthConfig) : BaseController
{
    [HttpPost]
    [Route("googleSignIn")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> GoogleSignIn(GoogleSignInVM model)
    {
        try
        {
            var data = await authService.SignInWithGoogle(model);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        await authService.Register(registerRequest);

        return Ok(new BaseResponse<string>("Inscription Réussie !"));
    }

    [HttpGet]
    [Route("confirm")]
    public async Task<IActionResult> Confirm([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            await authService.Confirm(email, token);
            return Redirect("http://localhost:4200/confirm");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            var data = await authService.Login(loginRequest);

            return Ok(data);
        }
        catch (LoginException e)
        {
            return Forbid();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("user-data")]
    public async Task<IActionResult> UserData()
    {
        var userData = await authService.GetUserData(User.GetNameIdentifierId());
        
        return Ok(userData);
    }
}