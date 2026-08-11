using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;
using SparkyProject.API.Services;

namespace SparkyProject.API.Controllers;

// Owner: Ruqaya
// Required cases (min. 8) — see capstone brief p.11-12:
// 1. POST   Create
// 2. PUT/PATCH  Update
// 3. PUT/PATCH  Second distinct update
// 4. DELETE Delete
// 5. GET (list) Include() related entity
// 6. GET (find) By Id
// 7. GET (filter) Where()
// 8. GET (sort/aggregate) OrderBy / Count / Sum / Average / GroupBy

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly IEmailService emailService;

    public BookingController(AppDbContext _context, IEmailService _emailService)
    {
        context = _context;
        emailService = _emailService;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
    {
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        await SendConfirmationEmailAsync(booking);
        return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, booking);
    }

    // Booking-confirmation email with the booking summary (domain trigger).
    private async Task SendConfirmationEmailAsync(Booking booking)
    {
        try
        {
            var user = await context.Users.FindAsync(booking.UserId);
            if (user == null) return;

            var body = $"<h3>Booking Confirmation</h3>" +
                       $"Booking ID: {booking.BookingId}<br/>" +
                       $"Room ID: {booking.RoomId}<br/>" +
                       $"Check-in: {booking.CheckInDate:yyyy-MM-dd}<br/>" +
                       $"Check-out: {booking.CheckOutDate:yyyy-MM-dd}<br/>" +
                       $"Status: {booking.Status}";

            await emailService.SendEmailAsync(user.UserEmail, "Booking Confirmation", body);
        }
        catch
        {
            // Email failure should never block the booking itself.
        }
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(int id, Booking updated)
    {
        var booking = await context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.UserId = updated.UserId;
        booking.RoomId = updated.RoomId;
        booking.PromotionId = updated.PromotionId;
        booking.CheckInDate = updated.CheckInDate;
        booking.CheckOutDate = updated.CheckOutDate;
        booking.Status = updated.Status;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - status update
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus)
    {
        var booking = await context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.Status = newStatus;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        context.Bookings.Remove(booking);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related entities
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings()
    {
        return await context.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .Include(b => b.Promotion)
            .ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Booking>> GetBooking(int id)
    {
        var booking = await context.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .Include(b => b.Promotion)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null) return NotFound();
        return booking;
    }

    // 7. GET (filter) - by status
    [HttpGet("by-status/{status}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Booking>>> GetByStatus(string status)
    {
        return await context.Bookings
            .Where(b => b.Status == status)
            .ToListAsync();
    }

    // 8. GET (sort) - by check-in date
    [HttpGet("sorted-by-checkin")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Booking>>> GetSortedByCheckIn()
    {
        return await context.Bookings
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }
}