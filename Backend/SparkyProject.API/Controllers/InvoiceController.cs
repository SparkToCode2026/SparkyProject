using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkyProject.API.Data;
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
public class InvoiceController : ControllerBase
{
    private readonly AppDbContext context;

    public InvoiceController(AppDbContext _context)
    {
        context = _context;
    }

    // TODO: implement the 8 cases above

    // 1. POST: api/Invoice
    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
    {
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.InvoiceId }, invoice);
    }

    // 2. PUT: api/Invoice/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvoice(int id, Invoice updatedInvoice)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        invoice.TotalAmount = updatedInvoice.TotalAmount;
        invoice.IssuedAt = updatedInvoice.IssuedAt;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 3. PUT: api/Invoice/5/amount
    [HttpPut("{id}/amount")]
    public async Task<IActionResult> UpdateInvoiceAmount(int id, [FromBody] decimal newAmount)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        invoice.TotalAmount = newAmount;
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 4. DELETE: api/Invoice/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound();

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. GET: api/Invoice (with Include for Booking navigation property)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetAllInvoices()
    {
        return await context.Invoices.Include(i => i._booking).ToListAsync();
    }

    // 6. GET: api/Invoice/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Invoice>> GetInvoiceById(int id)
    {
        var invoice = await context.Invoices.Include(i => i._booking)
            .FirstOrDefaultAsync(i => i.InvoiceId == id);

        if (invoice == null) return NotFound();
        return invoice;
    }

    // 7. GET: api/Invoice/filter?minAmount=100
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<Invoice>>> FilterInvoices(decimal minAmount)
    {
        return await context.Invoices
            .Where(i => i.TotalAmount >= minAmount)
            .ToListAsync();
    }

    // 8. GET: api/Invoice/summary (aggregate: total, average, count)
    [HttpGet("summary")]
    public async Task<ActionResult> GetInvoiceSummary()
    {
        var summary = new
        {
            TotalInvoices = await context.Invoices.CountAsync(),
            TotalAmount = await context.Invoices.SumAsync(i => i.TotalAmount),
            AverageAmount = await context.Invoices.AverageAsync(i => i.TotalAmount)
        };
        return Ok(summary);
    }
}