using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Areas.Admin.Controllers
{
    public class TimeSlotsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
