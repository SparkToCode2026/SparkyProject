using Microsoft.AspNetCore.Authorization;
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
// 7. GET (filter) LINQ Where()
// 8. GET (sort/aggregate) OrderBy / Count / Sum / Average / GroupBy

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext context;

    public PaymentController(AppDbContext _context)
    {
        context = _context;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
    {
        context.Payments.Add(payment);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, payment);
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, Payment updated)
    {
        var payment = await context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        payment.BookingId = updated.BookingId;
        payment.Amount = updated.Amount;
        payment.Method = updated.Method;
        payment.PaidAt = updated.PaidAt;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - amount update
    [HttpPatch("{id}/amount")]
    public async Task<IActionResult> UpdateAmount(int id, decimal newAmount)
    {
        var payment = await context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        payment.Amount = newAmount;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var payment = await context.Payments.FindAsync(id);
        if (payment == null) return NotFound();

        context.Payments.Remove(payment);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related Booking
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments()
    {
        return await context.Payments
            .Include(p => p.Booking)
            .ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        var payment = await context.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.PaymentId == id);

        if (payment == null) return NotFound();
        return payment;
    }

    // 7. GET (filter) - by method
    [HttpGet("by-method/{method}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Payment>>> GetByMethod(string method)
    {
        return await context.Payments
            .Where(p => p.Method == method)
            .ToListAsync();
    }

    // 8. GET (sort) - by amount
    [HttpGet("sorted-by-amount")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Payment>>> GetSortedByAmount()
    {
        return await context.Payments
            .OrderBy(p => p.Amount)
            .ToListAsync();
    }
}