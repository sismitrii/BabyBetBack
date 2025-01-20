using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Configuration;
using Application.Dtos.In;
using Application.Dtos.Out;
using Application.Exceptions;
using Application.Utils;
using Core.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SendGrid;
using SendGrid.Helpers.Mail;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Application.Services.Auth;

public class AuthService(
    IGoogleAuthService googleAuthService,
    UserManager<User> userManager,
    IOptions<JwtConfiguration> jwtConfig) : IAuthService
{
    private readonly JwtConfiguration _jwtConfig = jwtConfig.Value;

    public async Task<JwtResponseDto> SignInWithGoogle(GoogleSignInVM model)
    {
        var response = await googleAuthService.GoogleSignIn(model);
        
        if (response.Errors.Any())
            throw new LoginException("Failed to sign in with Google");

        var jwtResponse = CreateJwtToken(response.Data);

        var data = new JwtResponseDto
        {
            Token = jwtResponse,
        };

        return data;
    }

    public async Task<string> Register(RegisterRequest request)
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
        
        return await SendEmail(user.Email, confirmationLink);
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
        
        var jwtResponse = CreateJwtToken(user);
        
        return new JwtResponseDto{ Token = jwtResponse};;

    }

    private async Task<string> SendEmail(string email, string confirmationLink)
    {
        var fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, "BabyBet");
        var to = new EmailAddress(email, "New User");
        var plainTextContent = $"Pour confirmer votre email veuillez clicker sur ce lien : {confirmationLink}" ;
        var htmlContent = $"<p>Pour confirmer votre email veuillez clicker sur ce lien : {confirmationLink}</p>";
        var subject = "Confirmez Votre Email";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

        return response.Body.ToString();
    }

    private string CreateJwtToken(User user)
    { 

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = Encoding.ASCII.GetBytes(secret);

        var userClaims = BuildUserClaims(user);

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
    
    private List<Claim> BuildUserClaims(User user)
    {
        var userClaims = new List<Claim>()
        {
            new(JwtClaimTypes.Id, user.Id.ToString()),
            new(JwtClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Email),
            new(JwtClaimTypes.GivenName, user.FirstName),
            new(JwtClaimTypes.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return userClaims;
    }
}