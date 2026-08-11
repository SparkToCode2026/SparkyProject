using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

namespace SparkyProject.API.Controllers;

// Owner: Ahmed Al Malki
// Required cases (min. 8) — see capstone brief p.11-12:
// 1. POST   Create
// 2. PUT/PATCH  Update
// 3. PUT/PATCH  Second distinct update (status change / update via related FK)
// 4. DELETE Delete (consider soft-delete)
// 5. GET (list)   Include() a related navigation property
// 6. GET (find)   By Id
// 7. GET (filter) LINQ Where() on a meaningful field
// 8. GET (sort/aggregate) OrderBy / Count / Sum / Average / GroupBy

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AmenityController : ControllerBase
{
    private readonly AppDbContext context;

    public AmenityController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Amenity>> CreateAmenity(Amenity amenity)
    {
        context.Amenities.Add(amenity);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAmenity), new { id = amenity.Id }, amenity);
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAmenity(int id, Amenity updated)
    {
        var amenity = await context.Amenities.FindAsync(id);
        if (amenity == null) return NotFound();

        amenity.Name = updated.Name;
        amenity.Price = updated.Price;
        amenity.HotelId = updated.HotelId;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - second distinct update (price only)
    [HttpPatch("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, decimal newPrice)
    {
        var amenity = await context.Amenities.FindAsync(id);
        if (amenity == null) return NotFound();

        amenity.Price = newPrice;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAmenity(int id)
    {
        var amenity = await context.Amenities.FindAsync(id);
        if (amenity == null) return NotFound();

        context.Amenities.Remove(amenity);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related Hotel
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Amenity>>> GetAllAmenities()
    {
        return await context.Amenities.Include(a => a.Hotel).ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Amenity>> GetAmenity(int id)
    {
        var amenity = await context.Amenities
            .Include(a => a.Hotel)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (amenity == null) return NotFound();
        return amenity;
    }

    // 7. GET (filter) - by hotel
    [HttpGet("by-hotel/{hotelId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Amenity>>> GetByHotel(int hotelId)
    {
        return await context.Amenities
            .Where(a => a.HotelId == hotelId)
            .ToListAsync();
    }

    // 8. GET (sort/aggregate) - sorted by price
    [HttpGet("sorted-by-price")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Amenity>>> GetSortedByPrice()
    {
        return await context.Amenities
            .OrderBy(a => a.Price)
            .ToListAsync();
    }

    [HttpGet("average-price")]
    [AllowAnonymous]
    public async Task<ActionResult<decimal>> GetAveragePrice()
    {
        if (!await context.Amenities.AnyAsync()) return 0;
        return await context.Amenities.AverageAsync(a => a.Price);
    }
}