using BabyBetBack.Auth;
using Core.Entities;
using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Extensions;

namespace BabyBetBack.Extension;

public static class UserManagerExtension
{
    public static async Task<User> CreateUserFromSocialLogin(this UserManager<User> userManager, BetDbContext context,
        CreateUserFromSocialLogin model, LoginProvider loginProvider)
    {
        //CHECKS IF THE USER HAS NOT ALREADY BEEN LINKED TO AN IDENTITY PROVIDER
        var user = await userManager.FindByLoginAsync(loginProvider.GetDisplayName(), model.LoginProviderSubject);

        if (user != null)
            return user;
        
        user = await userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                ProfilePicture = model.ProfilePicture,
            };
            
            await userManager.CreateAsync(user);
            //EMAIL IS CONFIRMED; IT IS COMING FROM AN IDENTITY PROVIDER            
            user.EmailConfirmed = true;

            await userManager.UpdateAsync(user);
            await context.SaveChangesAsync();
        }
        
        UserLoginInfo userLoginInfo = null;
        
        switch (loginProvider)
        {
            case LoginProvider.Google:
            case LoginProvider.Facebook:
            {
                userLoginInfo = new UserLoginInfo(loginProvider.GetDisplayName(), model.LoginProviderSubject, loginProvider.GetDisplayName().ToUpper());
            }
                break;
        }
        
        //ADDS THE USER TO AN IDENTITY PROVIDER
        var result = await userManager.AddLoginAsync(user, userLoginInfo);

        return result.Succeeded ? user : null;
    }
}