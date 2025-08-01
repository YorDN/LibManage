using LibManage.Data.Models.Library;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LibManage.Web.Areas.Identity.Custom
{
    public class LibrarySignInManager : SignInManager<User>
    {
        public LibrarySignInManager(UserManager<User> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<User> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<User>> logger, 
            IAuthenticationSchemeProvider schemes, 
            IUserConfirmation<User> confirmation) 
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }
        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
                return SignInResult.Failed;

            if (!user.IsActive)
                return SignInResult.NotAllowed;

            return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }

    }
}
