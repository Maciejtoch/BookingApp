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

        // GET: My reservations
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

        // GET: Create reservation
        public async Task<IActionResult> Create(int? serviceId)
        {
            ViewBag.Services = new SelectList(
                await _context.Services.Where(s => s.IsActive).ToListAsync(),
                "Id", "Name", serviceId);

            if (serviceId.HasValue)
            {
                ViewBag.TimeSlots = await _context.TimeSlots
                    .Where(t => t.ServiceId == serviceId && t.IsAvailable && t.DateTime > DateTime.Now)
                    .OrderBy(t => t.DateTime)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.DateTime.ToString("dddd, dd MMM yyyy HH:mm")
                    })
                    .ToListAsync();
            }

            return View(new Reservation { ServiceId = serviceId ?? 0 });
        }

        // POST: Create reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            reservation.UserId = _userManager.GetUserId(User);
            reservation.CreatedAt = DateTime.Now;
            reservation.Status = ReservationStatus.Pending;

            // Get date from time slot
            if (reservation.TimeSlotId.HasValue)
            {
                var slot = await _context.TimeSlots.FindAsync(reservation.TimeSlotId);
                if (slot == null || !slot.IsAvailable)
                {
                    ModelState.AddModelError("", "Wybrany termin jest niedostępny.");
                }
                else
                {
                    reservation.Date = slot.DateTime;
                    slot.IsAvailable = false;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Rezerwacja została złożona! Oczekuje na zatwierdzenie.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Services = new SelectList(await _context.Services.Where(s => s.IsActive).ToListAsync(), "Id", "Name");
            return View(reservation);
        }

        // GET: Edit
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

        // POST: Edit
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
            TempData["Success"] = "Rezerwacja została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Cancel
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

            // Free up the time slot
            if (reservation.TimeSlot != null)
                reservation.TimeSlot.IsAvailable = true;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezerwacja została anulowana.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get time slots for service
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