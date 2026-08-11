using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

namespace SparkyProject.API.Controllers;

// Owner: Ahmed Al Malki
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly AppDbContext context;

    public RoomController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Room>> CreateRoom(Room room)
    {
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, room);
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, Room updated)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        room.RoomNumber = updated.RoomNumber;
        room.Status = updated.Status;
        room.RoomTypeId = updated.RoomTypeId;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - second distinct update (status change)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        room.Status = newStatus;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room == null) return NotFound();

        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related RoomType
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Room>>> GetAllRooms()
    {
        return await context.Rooms.Include(r => r._roomType).ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        var room = await context.Rooms
            .Include(r => r._roomType)
            .FirstOrDefaultAsync(r => r.RoomId == id);

        if (room == null) return NotFound();
        return room;
    }

    // 7. GET (filter) - by status
    [HttpGet("by-status/{status}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Room>>> GetByStatus(string status)
    {
        return await context.Rooms
            .Where(r => r.Status == status)
            .ToListAsync();
    }

    // 8. GET (sort/aggregate) - sorted by room number
    [HttpGet("sorted-by-number")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Room>>> GetSortedByNumber()
    {
        return await context.Rooms
            .OrderBy(r => r.RoomTypeId)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }
}