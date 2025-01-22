using Application.Configuration;
using Application.Dtos.In;
using Application.Exceptions;
using Application.Services.Auth;
using Application.Utils;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using LoginRequest = Application.Dtos.In.LoginRequest;
using RegisterRequest = Application.Dtos.In.RegisterRequest;

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
            var frontDomain = Environment.GetEnvironmentVariable("FRONT_DOMAIN");
            return Redirect($"{frontDomain}/confirm");
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
    
    [HttpPost]
    [Route("forgot-password")]
    public async Task<ActionResult> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        await authService.SendForgotPasswordLinkAsync(request);

        return Ok();
    }

    [HttpPost]
    [Route("reset-password")]
    public async Task<ActionResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        await authService.ResetPasswordAsync(request);
        
        return Ok("Mot de passe réinitialisé avec succes !");
    }
}