using System.ComponentModel.DataAnnotations;

namespace SparkyProject.API.Models;

// Owner: Aisha Mubarak ALHashmi
public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime IssueDate { get; set; } = DateTime.Now;

    public string Status { get; set; } = "Unpaid";

    public Booking? Booking { get; set; }
}