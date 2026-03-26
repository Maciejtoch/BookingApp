using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
