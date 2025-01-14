using System.IdentityModel.Tokens.Jwt;
using DAL;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using BabyBetBack.Configuration;
using BabyBetBack.Utils;
using Core.Entities;
using IdentityModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BabyBetBack.Auth;

public class AuthService(
    BetDbContext betDbContext,
    IGoogleAuthService googleAuthService,
    UserManager<User> userManager,
    IOptions<JwtConfiguration> jwtConfig) : IAuthService
{
    private readonly BetDbContext _betDbContext = betDbContext;
    private readonly IGoogleAuthService _googleAuthService = googleAuthService;
    private readonly UserManager<User> _userManager = userManager;
    private readonly JwtConfiguration _jwtConfig = jwtConfig.Value;

    public async Task<BaseResponse<JwtResponseVM>> SignInWithGoogle(GoogleSignInVM model)
    {
        var response = await _googleAuthService.GoogleSignIn(model);
        
        if (response.Errors.Any())
            return new BaseResponse<JwtResponseVM>(response.ResponseMessage, response.Errors);

        var jwtResponse = CreateJwtToken(response.Data);

        var data = new JwtResponseVM
        {
            Token = jwtResponse,
        };

        return new BaseResponse<JwtResponseVM>(data);
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
            new Claim(JwtClaimTypes.Id, user.Id.ToString()),
            new Claim(JwtClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            new Claim(JwtClaimTypes.GivenName, user.FirstName),
            new Claim(JwtClaimTypes.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return userClaims;
    }
}