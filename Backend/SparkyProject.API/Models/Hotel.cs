using System.ComponentModel.DataAnnotations;

namespace SparkyProject.API.Models;

// Owner: Ibrahim Al Kindi
public class Hotel
{
    [Key]
    public int HotelId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public int? UserId { get; set; }

    // 1:N Relationship with RoomType
    public List<RoomType> RoomTypes { get; set; } = new();

    // 1:N Relationship with Promotion
    public List<Promotion> PromotionTypes { get; set; } = new();
}