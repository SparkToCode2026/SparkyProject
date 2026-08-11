using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Aisha Mubarak ALHashmi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Invoice
{
    public int InvoiceId { get; set; }

    [ForeignKey("_booking")]
    public int BookingId { get; set; }
    public Booking _booking { get; set; } = null!;

    public decimal TotalAmount { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.Now;
}
