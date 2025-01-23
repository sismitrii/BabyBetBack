using Application.Configuration;
using Application.Dtos.In;
using Application.Extension;
using Application.Utils;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Application.Services.Auth;

public class GoogleAuthService(
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IOptions<GoogleAuthConfig> googleAuthConfig,
    ILogger<GoogleAuthConfig> logger)
    : IGoogleAuthService
{
    private readonly GoogleAuthConfig _googleAuthConfig = googleAuthConfig.Value;

    public async Task<BaseResponse<User>> GoogleSignIn(GoogleSignInVM model) // on ne serait peut être pas obligé d'utilisé d'objet
    {
     Payload payload = new();

     try
     {
         payload = await ValidateAsync(model.IdToken, new ValidationSettings
         {
             Audience = new[] { _googleAuthConfig.ClientId }
         });
     }
     catch (Exception e)
     {
         logger.LogError(e, e.Message);
         return new BaseResponse<User>(null, ["Failed to get a response"]);
     }

     var userToBeCreated = new CreateUserFromSocialLogin
     {
         FirstName = payload.GivenName,
         LastName = payload.FamilyName,
         Email = payload.Email,
         ProfilePicture = payload.Picture,
         LoginProviderSubject = payload.Subject,
     };
     
     var user = await userManager.CreateUserFromSocialLogin(unitOfWork, userToBeCreated, LoginProvider.Google);

     logger.LogInformation($"User created with google : {user.ToString()} ");
     
     return user != null ?
         new BaseResponse<User>(user) :
         new BaseResponse<User>(null, ["Unable to link a Local User to a Provider"]);
    }
}