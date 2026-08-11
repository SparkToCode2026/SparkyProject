using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

namespace SparkyProject.API.Controllers;

// Owner: Ibrahim Al Kindi
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class HotelController : ControllerBase
{
    private readonly AppDbContext context;

    public HotelController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Hotel>> CreateHotel(Hotel hotel)
    {
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetHotel), new { id = hotel.HotelId }, hotel);
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHotel(int id, Hotel updated)
    {
        var hotel = await context.Hotels.FindAsync(id);
        if (hotel == null) return NotFound();

        hotel.Name = updated.Name;
        hotel.Address = updated.Address;
        hotel.City = updated.City;
        hotel.Phone = updated.Phone;
        hotel.UserId = updated.UserId;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - second distinct update (change city)
    [HttpPatch("{id}/city")]
    public async Task<IActionResult> UpdateCity(int id, string newCity)
    {
        var hotel = await context.Hotels.FindAsync(id);
        if (hotel == null) return NotFound();

        hotel.City = newCity;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await context.Hotels.FindAsync(id);
        if (hotel == null) return NotFound();

        context.Hotels.Remove(hotel);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related RoomTypes
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetAllHotels()
    {
        return await context.Hotels.Include(h => h.RoomTypes).ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Hotel>> GetHotel(int id)
    {
        var hotel = await context.Hotels
            .Include(h => h.RoomTypes)
            .Include(h => h.PromotionTypes)
            .FirstOrDefaultAsync(h => h.HotelId == id);

        if (hotel == null) return NotFound();
        return hotel;
    }

    // 7. GET (filter) - by city
    [HttpGet("by-city/{city}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetByCity(string city)
    {
        return await context.Hotels
            .Where(h => h.City == city)
            .ToListAsync();
    }

    // 8. GET (sort/aggregate) - sorted by name
    [HttpGet("sorted-by-name")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetSortedByName()
    {
        return await context.Hotels
            .OrderBy(h => h.Name)
            .ToListAsync();
    }
}