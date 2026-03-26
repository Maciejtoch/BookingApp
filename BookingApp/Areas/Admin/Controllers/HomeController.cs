using BookingApp.Data;
using BookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalReservations = await _context.Reservations.CountAsync();
            ViewBag.PendingReservations = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Pending);
            ViewBag.ApprovedReservations = await _context.Reservations.CountAsync(r => r.Status == ReservationStatus.Approved);
            ViewBag.TotalServices = await _context.Services.CountAsync();
            ViewBag.RecentReservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Service)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();
            return View();
        }
    }
}