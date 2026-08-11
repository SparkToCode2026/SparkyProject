namespace SparkyProject.API.Models;

// Owner: Ruqaya
public class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }
    public int RoomId { get; set; }
    public int? PromotionId { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = "Pending";

    public User? User { get; set; }
    public Room? Room { get; set; }
    public Promotion? Promotion { get; set; }
    public List<Payment> Payments { get; set; } = new();
}