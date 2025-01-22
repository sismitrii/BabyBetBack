using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Configuration;
using Application.Dtos.In;
using Application.Dtos.Out;
using Application.Exceptions;
using AutoMapper;
using Core.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
using LoginRequest = Application.Dtos.In.LoginRequest;
using RegisterRequest = Application.Dtos.In.RegisterRequest;

namespace Application.Services.Auth;

public class AuthService(
    IGoogleAuthService googleAuthService,
    UserManager<User> userManager,
    IOptions<JwtConfiguration> jwtConfig,
    IMapper mapper) : IAuthService
{
    private readonly JwtConfiguration _jwtConfig = jwtConfig.Value;

    public async Task<JwtResponseDto> SignInWithGoogle(GoogleSignInVM model)
    {
        var response = await googleAuthService.GoogleSignIn(model);
        
        if (response.Errors.Any())
            throw new LoginException("Failed to sign in with Google");

        var jwtResponse = await CreateJwtTokenAsync(response.Data);

        var data = new JwtResponseDto
        {
            Token = jwtResponse,
        };

        return data;
    }

    public async Task Register(RegisterRequest request)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        
        if (existingUser != null)
            throw new Exception("An account already exists with this email.");

        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };
        
        var result = await userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description); //TODO custom errors
        
        var confirmEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        confirmEmailToken = Uri.EscapeDataString(confirmEmailToken);
        var domainName = Environment.GetEnvironmentVariable("DOMAIN_NAME");
        var confirmationLink = $"{domainName}/api/auth/confirm?token={confirmEmailToken}&email={request.Email}";

        var message = $"Pour confirmer votre email veuillez clicker sur ce lien : {confirmationLink}";
        var subject = "Confirmer mon Email";
        await SendEmail(user.Email, message, subject);
        //TODO Improve confirm message
    }

    public async Task Confirm(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email) ??
                   throw new Exception($"User with email {email} does not exist.");
        
        var result = await userManager.ConfirmEmailAsync(user, token) ??
                     throw new Exception($"Fail to confirm email {email}.");
        
        if (result.Errors.Any())
            throw new Exception(result.Errors.First().Description); // TODO Better exception pls
    }

    public async Task<JwtResponseDto> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new LoginException($"Les information de connection sont incorrect");
        
        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new NotConfirmEmailException("Veuillez confirmer votre email avant de vous connecter.");
        
        var jwtResponse = await CreateJwtTokenAsync(user);
        
        return new JwtResponseDto{ Token = jwtResponse};;

    }

    public async Task<UserDto> GetUserData(string userEmail)
    {
        var user = await userManager.FindByEmailAsync(userEmail);

        return mapper.Map<UserDto>(user);
    }

    public async Task SendForgotPasswordLinkAsync(ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.IsEmailConfirmedAsync(user))
            return; 
            
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        token = Uri.EscapeDataString(token);

        var frontDomain = Environment.GetEnvironmentVariable("FRONT_DOMAIN");
        var confirmationLink = $"{frontDomain}/resetPassword?email={user.Email}&token={token}";
        var message = $"Afin de modifier votre email, veuillez clicker sur le lien suivant : {confirmationLink}";
        var subject = "Reinitialiser mon mot de passe";
        await SendEmail(user.Email, message, subject);
        
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new LoginException($"User with email {request.Email} does not exist.");

        var token = Uri.UnescapeDataString(request.ResetCode);
        
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        
        if (!result.Succeeded)
            throw new LoginException("Error resetting password");
    }

    private async Task SendEmail(string email, string message, string subject)
    {
        var fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "BabyBet");
        var to = new EmailAddress(email);
        
        var plainTextContent = message ;
 ;
        var htmlContent = $"<p>{message}</p>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
    }

    private async Task<string> CreateJwtTokenAsync(User user)
    { 

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = Encoding.ASCII.GetBytes(secret);

        var userClaims = await BuildUserClaimsAsync(user);

        var signKey = new SymmetricSecurityKey(key);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwtConfig.ValidIssuer,
            notBefore: DateTime.UtcNow,
            audience: _jwtConfig.ValidAudience,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_jwtConfig.DurationInMinutes)),
            claims: userClaims,
            signingCredentials: new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }
    
    private async Task<List<Claim>> BuildUserClaimsAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var userClaims = new List<Claim>()
        {
            new(JwtClaimTypes.Id, user.Id.ToString()),
            new(JwtClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Email),
            new(JwtClaimTypes.GivenName, user.FirstName),
            new(JwtClaimTypes.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        
        userClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        

        return userClaims;
    }
}