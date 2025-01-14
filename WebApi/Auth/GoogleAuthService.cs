using BabyBetBack.Configuration;
using BabyBetBack.Extension;
using BabyBetBack.Utils;
using Core.Entities;
using DAL;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace BabyBetBack.Auth;

public class GoogleAuthService(
    UserManager<User> userManager,
    BetDbContext betDbContext,
    IOptions<GoogleAuthConfig> googleAuthConfig
    )
    : IGoogleAuthService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly BetDbContext _betDbContext = betDbContext;
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
         return new BaseResponse<User>(null, ["Failed to get a response"]);
     }

     var userToBeCreated = new CreateUserFromSocialLogin()
     {
         FirstName = payload.GivenName,
         LastName = payload.FamilyName,
         Email = payload.Email,
         ProfilePicture = payload.Picture,
         LoginProviderSubject = payload.Subject,
     };
     
     var user = await _userManager.CreateUserFromSocialLogin(_betDbContext, userToBeCreated, LoginProvider.Google);

     return user != null ?
         new BaseResponse<User>(user) :
         new BaseResponse<User>(null, ["Unable to link a Local User to a Provider"]);
    }
}