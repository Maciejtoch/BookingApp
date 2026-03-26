using BookingApp.Data;
using BookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var reservations = await _context.Reservations
                .Include(r => r.Service)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
            return View(reservations);
        }

        public async Task<IActionResult> Create(int? serviceId)
        {
            ViewBag.Services = new SelectList(
                await _context.Services.Where(s => s.IsActive).ToListAsync(),
                "Id", "Name", serviceId);
            return View(new Reservation { ServiceId = serviceId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int ServiceId, [FromForm] int? TimeSlotId, [FromForm] string Notes)
        {
            bool errors = false;

            if (ServiceId == 0)
            {
                ModelState.AddModelError("", "Musisz wybrać usługę.");
                errors = true;
            }

            if (!TimeSlotId.HasValue || TimeSlotId == 0)
            {
                ModelState.AddModelError("", "Musisz wybrać termin wizyty.");
                errors = true;
            }

            TimeSlot slot = null;
            if (!errors && TimeSlotId.HasValue)
            {
                slot = await _context.TimeSlots.FindAsync(TimeSlotId);
                if (slot == null || !slot.IsAvailable)
                {
                    ModelState.AddModelError("", "Wybrany termin jest już niedostępny. Wybierz inny.");
                    errors = true;
                }
            }

            if (errors)
            {
                ViewBag.Services = new SelectList(
                    await _context.Services.Where(s => s.IsActive).ToListAsync(), "Id", "Name", ServiceId);
                return View(new Reservation { ServiceId = ServiceId, Notes = Notes });
            }

            slot.IsAvailable = false;

            var reservation = new Reservation
            {
                UserId = _userManager.GetUserId(User),
                ServiceId = ServiceId,
                TimeSlotId = TimeSlotId,
                Date = slot.DateTime,
                Notes = Notes,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została złożona! Oczekuje na zatwierdzenie przez salon.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reservation == null) return NotFound();
            if (reservation.Status != ReservationStatus.Pending)
            {
                TempData["Error"] = "Można edytować tylko rezerwacje oczekujące.";
                return RedirectToAction(nameof(Index));
            }
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string notes)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (reservation == null) return NotFound();
            reservation.Notes = notes;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Notatka została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .Include(r => r.TimeSlot)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (reservation == null) return NotFound();
            reservation.Status = ReservationStatus.Cancelled;
            if (reservation.TimeSlot != null)
                reservation.TimeSlot.IsAvailable = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezerwacja została anulowana.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetTimeSlots(int serviceId)
        {
            var slots = await _context.TimeSlots
                .Where(t => t.ServiceId == serviceId && t.IsAvailable && t.DateTime > DateTime.Now)
                .OrderBy(t => t.DateTime)
                .Select(t => new { t.Id, label = t.DateTime.ToString("dddd, dd MMM yyyy HH:mm") })
                .ToListAsync();
            return Json(slots);
        }
    }
}