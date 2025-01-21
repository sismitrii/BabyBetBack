using Application.Dtos.In;
using Application.Dtos.Out;

namespace Application.Services.Auth;

public interface IAuthService
{
    Task<JwtResponseDto> SignInWithGoogle(GoogleSignInVM model);
    Task Register(RegisterRequest request);
    Task Confirm(string email, string token);
    Task<JwtResponseDto> Login(LoginRequest request);
    
    Task<UserDto> GetUserData(string userEmail);
}