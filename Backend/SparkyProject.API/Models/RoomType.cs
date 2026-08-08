using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SparkyProject.API.Models;

// Owner: Khalid Al Hashemi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class RoomType
{
    [Key]
    [JsonIgnore]
    public int RoomTypeId { get; set; }

    [Required]
    public string RoomName { get; set; }

    [Required]
    public double BasePrice { get; set; }

    [Required]
    public int Capacity { get; set; }


    // Foreign key to Hotel 1 : N relationship
    [ForeignKey("_hotel")]
    public int HotelId { get; set; }
    [JsonIgnore]
    public Hotel _hotel { get; set; }


    // 1:N Relationship with Room
    [JsonIgnore]
    public List<Room> Rooms { get; set; }
}
