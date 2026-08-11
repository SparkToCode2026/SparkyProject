using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;

namespace SparkyProject.API.Controllers;

// Owner: Aisha Mubarak ALHashmi
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StaffController : ControllerBase
{
    private readonly AppDbContext context;

    public StaffController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Staff>> CreateStaff(Staff staff)
    {
        context.Staff.Add(staff);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetStaff), new { id = staff.StaffId }, staff);
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStaff(int id, Staff updated)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        staff.FullName = updated.FullName;
        staff.Position = updated.Position;
        staff.Email = updated.Email;
        staff.Phone = updated.Phone;
        staff.UserId = updated.UserId;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - second distinct update (position change)
    [HttpPatch("{id}/position")]
    public async Task<IActionResult> UpdatePosition(int id, string newPosition)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        staff.Position = newPosition;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var staff = await context.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        context.Staff.Remove(staff);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related User
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Staff>>> GetAllStaff()
    {
        return await context.Staff.Include(s => s.User).ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Staff>> GetStaff(int id)
    {
        var staff = await context.Staff
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.StaffId == id);

        if (staff == null) return NotFound();
        return staff;
    }

    // 7. GET (filter) - by position
    [HttpGet("by-position/{position}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Staff>>> GetByPosition(string position)
    {
        return await context.Staff
            .Where(s => s.Position == position)
            .ToListAsync();
    }

    // 8. GET (sort/aggregate) - sorted by hire date
    [HttpGet("sorted-by-hire-date")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Staff>>> GetSortedByHireDate()
    {
        return await context.Staff
            .OrderByDescending(s => s.HireDate)
            .ToListAsync();
    }
}