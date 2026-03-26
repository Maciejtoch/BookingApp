using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
