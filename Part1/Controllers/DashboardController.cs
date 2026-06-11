using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;

namespace EventEase.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalEvents = await _context.Events.CountAsync();
            ViewBag.TotalVenues = await _context.Venues.CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            ViewBag.UpcomingEvents = await _context.Events
                .Include(e => e.Venue)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}