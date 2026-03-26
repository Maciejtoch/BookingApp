using BookingApp.Data;
using BookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TimeSlotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? serviceId)
        {
            var query = _context.TimeSlots.Include(t => t.Service).AsQueryable();
            if (serviceId.HasValue)
                query = query.Where(t => t.ServiceId == serviceId);

            ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name", serviceId);
            return View(await query.OrderBy(t => t.DateTime).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Services = new SelectList(await _context.Services.Where(s => s.IsActive).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimeSlot slot)
        {
            if (ModelState.IsValid)
            {
                _context.TimeSlots.Add(slot);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Termin został dodany.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");
            return View(slot);
        }

        // Bulk create - generate slots for date range
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(int serviceId, DateTime startDate, DateTime endDate, int startHour, int endHour, int intervalHours)
        {
            var current = startDate.Date;
            int count = 0;
            while (current <= endDate.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Sunday)
                {
                    for (int h = startHour; h <= endHour; h += intervalHours)
                    {
                        var dt = current.AddHours(h);
                        if (!await _context.TimeSlots.AnyAsync(t => t.ServiceId == serviceId && t.DateTime == dt))
                        {
                            _context.TimeSlots.Add(new TimeSlot { ServiceId = serviceId, DateTime = dt, IsAvailable = true });
                            count++;
                        }
                    }
                }
                current = current.AddDays(1);
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Dodano {count} terminów.";
            return RedirectToAction(nameof(Index), new { serviceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var slot = await _context.TimeSlots.FindAsync(id);
            if (slot != null) { _context.TimeSlots.Remove(slot); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Termin usunięty.";
            return RedirectToAction(nameof(Index));
        }
    }
}