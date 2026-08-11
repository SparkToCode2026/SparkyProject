using System.ComponentModel.DataAnnotations;

namespace SparkyProject.API.Models;

// Owner: Ibrahim Al Kindi
public class Review
{
    [Key]
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int? HotelId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User? User { get; set; }
    public Hotel? Hotel { get; set; }
}