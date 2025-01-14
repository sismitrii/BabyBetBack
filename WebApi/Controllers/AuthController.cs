using BabyBetBack.Auth;
using BabyBetBack.Utils;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace BabyBetBack.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseController
{
    private readonly IAuthService _authService = authService;
    [HttpPost]
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
}