using BabyBetBack.Utils;
using Core.Entities;
using DAL;

namespace BabyBetBack.Auth;

public interface IGoogleAuthService
{
    Task<BaseResponse<User>> GoogleSignIn(GoogleSignInVM model);

}