using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace LibManage.Web.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected bool IsUserAuthenticated()
        {
            return this.User.Identity?.IsAuthenticated ?? false;
        }
        protected string? GetUserId()
        {
            string? userId = null;
            bool isUserAuthenticated = IsUserAuthenticated();
            if (isUserAuthenticated)
            {
                userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            return userId;
        }
    }
}
