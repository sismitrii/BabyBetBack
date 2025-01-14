using BabyBetBack.Auth;
using BabyBetBack.Configuration;
using BabyBetBack.Utils;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BabyBetBack.Controllers;

[ApiController]
public class AuthController(IAuthService authService, IOptions<GoogleAuthConfig> googleAuthConfig) : BaseController
{
    private readonly IAuthService _authService = authService;
    [HttpPost]
    [Route("api/auth")]
    [ProducesResponseType(typeof(BaseResponse<bool>), 200)]
    public async Task<IActionResult> GoogleSignIn(GoogleSignInVM model)
    {
        try
        {
            var data = await _authService.SignInWithGoogle(model);
            return Ok(data.Data);
        }
        catch (Exception ex)
        {
            return HandleError(ex);
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        var secret = googleAuthConfig.Value.ClientSecret;
        return Ok(secret);
    }
}