using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Khalid Al Hashemi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class RoomType
{
    [Key]
    public int RoomTypeId { get; set; }
    public string RoomName { get; set; }
    public double BasePrice { get; set; }
    public int Capacity { get; set; }


    // Foreign key to Hotel 1 : N relationship
    [ForeignKey("_hotel")]
    public int HotelId { get; set; }
    public Hotel _hotel { get; set; }


    // 1:N Relationship with Room
    public List<Room> Rooms { get; set; }
}
