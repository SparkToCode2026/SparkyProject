using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

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
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDbContext context;

    public BookingController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
    {
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, booking);
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
    public async Task<ActionResult<IEnumerable<Booking>>> GetByStatus(string status)
    {
        return await context.Bookings
            .Where(b => b.Status == status)
            .ToListAsync();
    }

    // 8. GET (sort) - by check-in date
    [HttpGet("sorted-by-checkin")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetSortedByCheckIn()
    {
        return await context.Bookings
            .OrderBy(b => b.CheckInDate)
            .ToListAsync();
    }
}