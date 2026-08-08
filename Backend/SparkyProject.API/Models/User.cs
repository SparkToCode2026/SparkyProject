using System.ComponentModel.DataAnnotations;

namespace SparkyProject.API.Models;

// Owner: Murooj Al Shehaibi
// TODO: add properties (PK, FKs, navigation properties) per the team ERD.
public class User
{
    [Key]
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }

    // 1:1 Relationship with GuestProfile
    public GuestProfile _GuestProfile { get; set; }

    // 1:N Relationship with Hotel, Staff, Booking, and Review
    public List<Hotel> Hotels { get; set; }
    public List<Staff> Staffs { get; set; }
    public List<Booking> Bookings { get; set; }
    public List<Review> Reviews { get; set; }


}
