using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
using SparkyProject.API.Models;
using SparkyProject.API.Services;

namespace SparkyProject.API.Controllers;

// Owner: Aisha Mubarak ALHashmi
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly IEmailService emailService;

    public InvoiceController(AppDbContext _context, IEmailService _emailService)
    {
        context = _context;
        emailService = _emailService;
    }

    // 1. POST - create
    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
    {
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        await SendInvoiceEmailAsync(invoice);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceId }, invoice);
    }

    // Invoice email after checkout (domain trigger).
    private async Task SendInvoiceEmailAsync(Invoice invoice)
    {
        try
        {
            var booking = await context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingId == invoice.BookingId);
            if (booking?.User == null) return;

            var body = $"<h3>Your Invoice</h3>" +
                       $"Invoice ID: {invoice.InvoiceId}<br/>" +
                       $"Booking ID: {invoice.BookingId}<br/>" +
                       $"Total: {invoice.TotalAmount}<br/>" +
                       $"Status: {invoice.Status}";

            await emailService.SendEmailAsync(booking.User.UserEmail, "Your Invoice", body);
        }
        catch
        {
            // Email failure should never block the invoice itself.
        }
    }

    // 2. PUT - full update
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(int id, Invoice updated)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        invoice.BookingId = updated.BookingId;
        invoice.TotalAmount = updated.TotalAmount;
        invoice.IssueDate = updated.IssueDate;
        invoice.Status = updated.Status;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PATCH - second distinct update (payment status)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        invoice.Status = newStatus;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET (list) - includes related Booking
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetAllInvoices()
    {
        return await context.Invoices.Include(i => i.Booking).ToListAsync();
    }

    // 6. GET (find) - by Id
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Invoice>> GetInvoice(int id)
    {
        var invoice = await context.Invoices
            .Include(i => i.Booking)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);

        if (invoice == null) return NotFound();
        return invoice;
    }

    // 7. GET (filter) - by status
    [HttpGet("by-status/{status}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetByStatus(string status)
    {
        return await context.Invoices
            .Where(i => i.Status == status)
            .ToListAsync();
    }

    // 8. GET (sort/aggregate) - sorted by issue date
    [HttpGet("sorted-by-issue-date")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetSortedByIssueDate()
    {
        return await context.Invoices
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();
    }

    [HttpGet("total-revenue")]
    [AllowAnonymous]
    public async Task<ActionResult<decimal>> GetTotalRevenue()
    {
        if (!await context.Invoices.AnyAsync()) return 0;
        return await context.Invoices.SumAsync(i => i.TotalAmount);
    }
}