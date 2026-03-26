using BookingApp.Data;
using BookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = null)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Service)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && System.Enum.TryParse<ReservationStatus>(status, out var s))
                query = query.Where(r => r.Status == s);

            ViewBag.CurrentStatus = status;
            return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var r = await _context.Reservations.FindAsync(id);
            if (r != null) { r.Status = ReservationStatus.Approved; await _context.SaveChangesAsync(); }
            TempData["Success"] = "Rezerwacja zatwierdzona.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var r = await _context.Reservations
                .Include(r => r.TimeSlot)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (r != null)
            {
                r.Status = ReservationStatus.Rejected;
                if (r.TimeSlot != null) r.TimeSlot.IsAvailable = true;
                await _context.SaveChangesAsync();
            }
            TempData["Success"] = "Rezerwacja odrzucona.";
            return RedirectToAction(nameof(Index));
        }
    }
}