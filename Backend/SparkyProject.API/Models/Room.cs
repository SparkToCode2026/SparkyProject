using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Ahmed Almalki
public class Room
{
    [Key]
    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string Status { get; set; } = "Available";

    // Foreign key to RoomType 1 : N relationship
    [ForeignKey("_roomType")]
    public int RoomTypeId { get; set; }
    public RoomType _roomType { get; set; }
}