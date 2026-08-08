using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkyProject.API.Models;

// Owner: Khalid Al Hashemi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Promotion
{
    [Key]
    public int PromotionId { get; set; }
    public string PromotionCode { get; set; }
    public double DiscountPercentage { get; set; }
    public DateTime ExpiryDate { get; set; }



    // Foreign key to Hotel 1 : N relationship
    [ForeignKey("_Hotels")]
    public int HotelId { get; set; }
    public Hotel _Hotels { get; set; }

    

    // 1:N Relationship with Booking
    public List<Booking> Bookings { get; set; }
}
