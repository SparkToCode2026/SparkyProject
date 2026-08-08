using System.ComponentModel.DataAnnotations;

namespace SparkyProject.API.Models;

// Owner: Ibrahim Al Kindi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Hotel
{
    [Key]
    public int HotelId { get; set; }



    // 1:N Relationship with RoomType
    public List<RoomType> RoomTypes { get; set; }


    // 1:N Relationship with Promotion
    public List<Promotion> PromotionTypes { get; set; }
}
