using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Ahmed Almalki
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Room
{
    [Key]
    public int RoomId { get; set; }



    // Foreign key to RoomType 1 : N relationship
    [ForeignKey("_roomType")]
    public int RoomTypeId { get; set; }
    public RoomType _roomType { get; set; }
}
