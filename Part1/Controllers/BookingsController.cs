using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.ViewModels;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // BOOKINGS PAGE WITH SEARCH
        public async Task<IActionResult> Index(
     string searchString,
     DateTime? startDate,
     DateTime? endDate,
     int? eventTypeId,
     bool? isAvailable)
        {
            ViewBag.SearchString = searchString;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.IsAvailable = isAvailable?.ToString().ToLower();

            ViewBag.EventTypes = new SelectList(
                _context.EventTypes,
                "EventTypeId",
                "EventTypeName",
                eventTypeId);

            var bookings = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            // Search by Booking ID or Event Name
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(searchString) ||
                    b.Event.EventName.Contains(searchString));
            }

            // Start Date Filter
            if (startDate.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.BookingDate.Date >= startDate.Value.Date);
            }

            // End Date Filter
            if (endDate.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.BookingDate.Date <= endDate.Value.Date);
            }

            // Event Type Filter
            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.Event.EventTypeId == eventTypeId.Value);
            }

            // Venue Availability Filter
            if (isAvailable.HasValue)
            {
                bookings = bookings.Where(b =>
                    b.Venue.IsAvailable == isAvailable.Value);
            }

            var result = await bookings
                .OrderBy(b => b.BookingDate)
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    EventName = b.Event.EventName,
                    VenueName = b.Venue.VenueName,
                    EventDate = b.Event.EventDate,
                    BookingDate = b.BookingDate,

                    EventTypeName = b.Event.EventType.EventTypeName,

                    IsAvailable = b.Venue.IsAvailable
                })
                .ToListAsync();

            return View(result);
        
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // CREATE GET
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(
                _context.Events,
                "EventId",
                "EventName"
            );

            ViewData["VenueId"] = new SelectList(
                _context.Venues,
                "VenueId",
                "VenueName"
            );

            return View();
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            var selectedEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            // Validation: Booking date cannot be before event date
            if (selectedEvent != null &&
                booking.BookingDate.Date < selectedEvent.EventDate.Date)
            {
                ModelState.AddModelError(
                    "",
                    "Booking date cannot be before the event date."
                );
            }

            // Validation: Prevent double booking
            bool venueBooked = await _context.Bookings
                .Include(b => b.Event)
                .AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (venueBooked)
            {
                ModelState.AddModelError(
                    "",
                    "This venue is already booked for this event date."
                );
            }

            if (ModelState.IsValid)
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking created successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(
                _context.Events,
                "EventId",
                "EventName",
                booking.EventId
            );

            ViewData["VenueId"] = new SelectList(
                _context.Venues,
                "VenueId",
                "VenueName",
                booking.VenueId
            );

            return View(booking);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound();

            ViewData["EventId"] = new SelectList(
                _context.Events,
                "EventId",
                "EventName",
                booking.EventId
            );

            ViewData["VenueId"] = new SelectList(
                _context.Venues,
                "VenueId",
                "VenueName",
                booking.VenueId
            );

            return View(booking);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
                return NotFound();

            var selectedEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            // Validation: Booking date
            if (selectedEvent != null &&
                booking.BookingDate.Date < selectedEvent.EventDate.Date)
            {
                ModelState.AddModelError(
                    "",
                    "Booking date cannot be before the event date."
                );
            }

            // Validation: Prevent double booking
            bool venueBooked = await _context.Bookings
                .Include(b => b.Event)
                .AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.Event.EventDate.Date == selectedEvent.EventDate.Date &&
                    b.BookingId != booking.BookingId);

            if (venueBooked)
            {
                ModelState.AddModelError(
                    "",
                    "This venue is already booked for this event date."
                );
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Booking updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(
                _context.Events,
                "EventId",
                "EventName",
                booking.EventId
            );

            ViewData["VenueId"] = new SelectList(
                _context.Venues,
                "VenueId",
                "VenueName",
                booking.VenueId
            );

            return View(booking);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // CHECK IF BOOKING EXISTS
        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}