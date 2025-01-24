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
using Microsoft.Extensions.Logging;
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
    ILogger<AuthService> logger,
    IMapper mapper) : IAuthService
{
    private readonly JwtConfiguration _jwtConfig = jwtConfig.Value;

    public async Task<JwtResponseDto> SignInWithGoogle(GoogleSignInVM model)
    {
        var response = await googleAuthService.GoogleSignIn(model);

        if (response.Errors.Any())
        {
            logger.LogError($"Failed to sign in with Google : {string.Join(", ", response.Errors)}" );
            throw new AuthException("Failed to sign in with Google");
        }

        var jwtResponse = await CreateJwtTokenAsync(response.Data);

        var data = new JwtResponseDto
        {
            Token = jwtResponse,
        };

        return data;
    }

    public async Task Register(RegisterRequest request)
    {
        logger.LogDebug($"Register with request : {request}");
        var existingUser = await userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            var errorMessage = $"An account already exists with this email : {request.Email}.";
            logger.LogError(errorMessage);
            throw new AuthException(errorMessage);
        }

        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };
        
        logger.LogInformation($"User to be created : {user}");
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            logger.LogError($"Failed to create user: {result.Errors.FirstOrDefault()?.Description}");
            throw new AuthException(result.Errors.First().Description); //TODO custom errors
        }

        try
        {
            var confirmEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            confirmEmailToken = Uri.EscapeDataString(confirmEmailToken);
            var domainName = Environment.GetEnvironmentVariable("DOMAIN_NAME");
            var confirmationLink = $"{domainName}/api/auth/confirm?token={confirmEmailToken}&email={request.Email}";

            var message = $"Pour confirmer votre email veuillez clicker sur ce lien : {confirmationLink}";
            var subject = "Confirmer mon Email";
            await SendEmail(user.Email, message, subject);
            //TODO Improve confirm message
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to send email: {ex.Message}");
            await userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task Confirm(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email) ??
                   throw new AuthException($"User with email {email} does not exist.");
        
        var result = await userManager.ConfirmEmailAsync(user, token) ??
                     throw new AuthException($"Fail to confirm email {email}.");

        if (result.Errors.Any())
        {
            logger.LogError($"Error happened confirming email : {string.Join(", ", result.Errors.Select(x => x.Description))}");
            throw new AuthException(result.Errors.First().Description); // TODO Better exception pls
        }
    }

    public async Task<JwtResponseDto> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new AuthException($"Les information de connection sont incorrect");
        
        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new NotConfirmEmailException("Veuillez confirmer votre email avant de vous connecter.");
        
        var jwtResponse = await CreateJwtTokenAsync(user);
        
        return new JwtResponseDto{ Token = jwtResponse};;

    }

    public async Task<UserDto> GetUserData(string userEmail)
    {
        logger.LogDebug($"Get user data : {userEmail}");
        var user = await userManager.FindByEmailAsync(userEmail);

        return mapper.Map<UserDto>(user);
    }

    public async Task SendForgotPasswordLinkAsync(ForgotPasswordRequest request)
    {
        logger.LogDebug($"Send Forgot Password link : {request.Email}");
        
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.IsEmailConfirmedAsync(user))
        {
            logger.LogWarning($"Request for email : {request.Email} have been send but this user is not existing.");
            return; 
        }
        
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
        logger.LogDebug($"Reset Password for {request.Email}");
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new AuthException($"User with email {request.Email} does not exist.");

        var token = Uri.UnescapeDataString(request.ResetCode);
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        
        if (!result.Succeeded)
            throw new AuthException("Error resetting password");
    }

    public async Task DeleteUserAsync(string userEmail)
    {
        logger.LogDebug($"Start - Delete user : {userEmail}");
        var user = await userManager.FindByEmailAsync(userEmail) ??
                   throw new AuthException($"User with email {userEmail} does not exist.");
        
        var result = await userManager.DeleteAsync(user);

        if (result.Succeeded)
            logger.LogDebug($"User {userEmail} has been deleted");
        else
        {
            logger.LogError($"Failed to delete user {userEmail}");
            throw new AuthException($"Failed to delete user {userEmail}");
        }
    }

    private async Task SendEmail(string email, string message, string subject)
    {
        logger.LogDebug($"Send email to '{email}' with subject : '{subject}' and message :'{message}'");
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