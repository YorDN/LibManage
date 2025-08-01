using LibManage.Data.Models.Library;
using Microsoft.AspNetCore.Identity;

namespace LibManage.Web.Areas.Identity.Middleware
{
    public class ActiveUserMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            if(context.User.Identity?.IsAuthenticated == true)
            {
                User? user = await userManager.GetUserAsync(context.User);
                if (user != null && !user.IsActive)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Identity/Account/Login?message=deactivated");
                    return;
                }
            }
            await next(context);
        }
    }
}
