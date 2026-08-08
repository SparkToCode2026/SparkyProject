using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Ruqaya
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Booking
{
    [Key]
    public int BookingId { get; set; }



    // Foreign key to Promotion 1 : N relationship
    [ForeignKey("_Promotions")]
    public int PromotionId { get; set; }
    public Promotion _Promotions { get; set; }
}
