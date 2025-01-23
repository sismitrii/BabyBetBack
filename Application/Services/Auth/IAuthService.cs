using Application.Dtos.In;
using Application.Dtos.Out;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = Application.Dtos.In.LoginRequest;
using RegisterRequest = Application.Dtos.In.RegisterRequest;

namespace Application.Services.Auth;

public interface IAuthService
{
    Task<JwtResponseDto> SignInWithGoogle(GoogleSignInVM model);
    Task Register(RegisterRequest request);
    Task Confirm(string email, string token);
    Task<JwtResponseDto> Login(LoginRequest request);
    
    Task<UserDto> GetUserData(string userEmail);
    Task SendForgotPasswordLinkAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}