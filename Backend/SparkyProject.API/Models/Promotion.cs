using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SparkyProject.API.Models;

// Owner: Khalid Al Hashemi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class Promotion
{
    [Key]
    [JsonIgnore]
    public int PromotionId { get; set; }

    [Required]
    public string PromotionCode { get; set; }

    [Required]
    public double DiscountPercentage { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }



    // Foreign key to Hotel 1 : N relationship
    [ForeignKey("_Hotels")]
    public int HotelId { get; set; }

    [JsonIgnore]
    public Hotel _Hotels { get; set; }

    

    // 1:N Relationship with Booking
    [JsonIgnore]
    public List<Booking> Bookings { get; set; }
}
