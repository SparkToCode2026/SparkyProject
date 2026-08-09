using Microsoft.AspNetCore.Mvc;
using SparkyProject.API.Data;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Models;
namespace SparkyProject.API.Controllers;

// Owner: Aisha Mubarak ALHashmi
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
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly AppDbContext context;

    public StaffController(AppDbContext _context)
    {
        context = _context;
    }

    // TODO: implement the 8 cases above
    // 1. POST: api/Staff
    [HttpPost]
    public async Task<ActionResult<Staff>> CreateStaff(Staff staff)
    {
        context.Staff.Add(staff);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStaffById), new { id = staff.StaffId }, staff);
    }

    // 2. PUT: api/Staff/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStaff(int id, Staff updatedStaff)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        staff.Position = updatedStaff.Position;
        staff.HireDate = updatedStaff.HireDate;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PUT: api/Staff/5/position
    [HttpPut("{id}/position")]
    public async Task<IActionResult> UpdateStaffPosition(int id, [FromBody] string newPosition)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        staff.Position = newPosition;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE: api/Staff/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        context.Staff.Remove(staff);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET: api/Staff (with Include for Hotel navigation property)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Staff>>> GetAllStaff()
    {
        return await context.Staff.Include(s => s._hotel).ToListAsync();
    }

    // 6. GET: api/Staff/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Staff>> GetStaffById(int id)
    {
        var staff = await context.Staff.Include(s => s._hotel)
            .FirstOrDefaultAsync(s => s.StaffId == id);

        if (staff == null) return NotFound();
        return staff;
    }

    // 7. GET: api/Staff/filter?position=Manager
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<Staff>>> FilterStaffByPosition(string position)
    {
        return await context.Staff
            .Where(s => s.Position == position)
            .ToListAsync();
    }

    // 8. GET: api/Staff/summary (aggregate: count by hotel)
    [HttpGet("summary")]
    public async Task<ActionResult> GetStaffSummary()
    {
        var summary = await context.Staff
            .GroupBy(s => s.HotelId)
            .Select(g => new { HotelId = g.Key, StaffCount = g.Count() })
            .ToListAsync();

        return Ok(summary);
    }
}
