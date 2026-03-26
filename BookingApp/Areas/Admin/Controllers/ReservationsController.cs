using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Areas.Admin.Controllers
{
    public class ReservationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
