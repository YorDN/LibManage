using LibManage.Services.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LibManage.Web.Controllers
{
    public class PublishersController (ICountryService countryService) : BaseController
    {
        [Authorize(Roles ="Admin, Manager")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var countries = await countryService
                .GetCountriesAsync();
            var selectItems = countries.Select(c => new SelectListItem
            {
                Text = c.name.common,
                Value = c.cca2.ToUpper()
            }).ToList();

            var flagsDict = countries.ToDictionary(
                c => c.cca2.ToUpper(),
                c => c.flags.png
            );

            ViewBag.Countries = selectItems;
            ViewBag.CountryFlags = flagsDict;

            return View();

        }
    }
}
