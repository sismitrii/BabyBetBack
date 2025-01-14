using BabyBetBack.Utils;

namespace BabyBetBack.Auth;

public interface IAuthService
{
    Task<BaseResponse<JwtResponseVM>> SignInWithGoogle(GoogleSignInVM model);
}