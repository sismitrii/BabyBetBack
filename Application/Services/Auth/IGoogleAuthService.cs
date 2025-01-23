using Application.Dtos.In;
using Application.Utils;
using Core.Entities;

namespace Application.Services.Auth;

public interface IGoogleAuthService
{
    Task<BaseResponse<User>> GoogleSignIn(GoogleSignInVM model);

}