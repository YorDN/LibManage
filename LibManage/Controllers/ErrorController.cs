using Microsoft.AspNetCore.Mvc;

namespace LibManage.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult Error404()
        {
            return View("Error404");
        }
        [Route("Error/500")]
        public IActionResult ServerError()
        {
            return View("Error500");
        }
    }
}
